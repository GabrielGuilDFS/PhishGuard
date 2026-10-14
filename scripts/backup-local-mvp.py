"""Explicit local maintenance: prepare, verify, apply, resume. No secrets on stdout."""
from __future__ import annotations

import argparse
import csv
import hashlib
import json
import os
from pathlib import Path
import secrets
import socket
import subprocess
import sys
import time
from datetime import datetime, timezone
import zipfile

ROOT = Path(__file__).resolve().parents[1]
DB = "phishguard-db"
API = "phishguard-backend"
FRONT = "phishguard-frontend"
EXPECTED = "20261003030830_EnforceTenantRelationshipsAndCampaignSnapshots"
SKIP = {".git", ".backups", ".vs", "node_modules", "bin", "obj", "dist", "build",
        "TestResults", "test-results", "playwright-report", "__pycache__"}
EXTRA_COLUMNS = {
    "alvos": ["email_normalizado"],
    "campaign_target": ["tenant_id", "target_name_snapshot", "target_email_snapshot",
                        "department_snapshot", "snapshot_captured_at_utc", "is_legacy_snapshot"],
    "campaigns": ["snapshot_captured_at_utc", "is_legacy_snapshot", "template_name_snapshot",
                  "email_subject_snapshot", "email_body_snapshot", "sender_name_snapshot",
                  "sender_email_snapshot", "landing_name_snapshot", "landing_html_snapshot",
                  "landing_template_id_snapshot", "educational_name_snapshot",
                  "educational_html_snapshot", "educational_feedback_config_json_snapshot"],
}


def run(args: list[str], backup: Path, label: str, *, data: str | None = None,
        env: dict[str, str] | None = None, cwd: Path = ROOT) -> str:
    result = subprocess.run(args, cwd=cwd, env=env, input=data, text=True,
                            encoding="utf-8", errors="replace", capture_output=True)
    (backup / f"{label}.log").write_text(result.stdout + result.stderr, encoding="utf-8")
    if result.returncode:
        raise RuntimeError(f"Etapa {label} falhou; consulte o log privado do backup.")
    return result.stdout


def inspect(name: str, backup: Path) -> dict:
    text = run(["docker", "inspect", name], backup, f"inspect-{name}")
    (backup / f"{name}.json").write_text(text, encoding="utf-8")
    return json.loads(text)[0]


def environment(info: dict) -> dict[str, str]:
    return dict(item.split("=", 1) for item in info["Config"]["Env"] if "=" in item)


def psql(container: str, user: str, database: str, port: int,
         sql: str, backup: Path, label: str) -> str:
    return run(["docker", "exec", "-i", container, "psql", "-X", "-q", "-A", "-t",
                "-v", "ON_ERROR_STOP=1", "-p", str(port), "-U", user, "-d", database],
               backup, label, data=sql)


def fingerprint(container: str, settings: dict, backup: Path, label: str) -> str:
    cases = []
    for table, columns in EXTRA_COLUMNS.items():
        array = ",".join("'" + c + "'" for c in columns)
        cases.append(f"WHEN '{table}' THEN ARRAY[{array}]::text[]")
    # Identifiers come from pg_tables and are quoted by PostgreSQL's format %I.
    # Added snapshot columns are excluded to compare the original records after upgrade.
    sql = """SET TIME ZONE 'UTC';
    CREATE TEMP TABLE recovery_fingerprints (table_name text, row_count bigint, digest text);
    DO $$ DECLARE t record; cols text[]; n bigint; d text; BEGIN
      FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='public'
               AND tablename <> '__EFMigrationsHistory' ORDER BY tablename LOOP
        cols := CASE t.tablename """ + " ".join(cases) + """ ELSE ARRAY[]::text[] END;
        EXECUTE format('SELECT count(*), md5(coalesce(string_agg(h, '''' ORDER BY h), ''''))
                        FROM (SELECT md5((to_jsonb(r) - $1)::text) AS h FROM public.%I r) q', t.tablename)
          INTO n,d USING cols;
        INSERT INTO recovery_fingerprints VALUES (t.tablename,n,d);
      END LOOP;
    END $$;
    SELECT table_name || '|' || row_count || '|' || digest FROM recovery_fingerprints ORDER BY table_name;
    """
    return psql(container, settings["user"], settings["database"], settings["port"], sql, backup, label).strip()


def audit(container: str, settings: dict, backup: Path, label: str) -> None:
    result = psql(container, settings["user"], settings["database"], settings["port"],
                  (ROOT / "scripts/check-mvp-integrity.sql").read_text(encoding="utf-8"), backup, label)
    if result.strip():
        raise RuntimeError("Diagnóstico encontrou conflitos. Dados preservados; revise o relatório privado antes de migrar.")


def save_manifest(backup: Path, manifest: dict) -> None:
    (backup / "manifest.json").write_text(json.dumps(manifest, indent=2, ensure_ascii=False), encoding="utf-8")


def ensure_stopped(backup: Path) -> None:
    state = run(["docker", "inspect", "--format", "{{.State.Running}}", API], backup, "api-stopped-check")
    if state.strip() != "false":
        raise RuntimeError("A API precisa estar parada durante esta manutenção.")


def verify_backup_files(backup: Path, manifest: dict) -> None:
    for name, expected in manifest["hashes"].items():
        digest = hashlib.sha256()
        with (backup / name).open("rb") as stream:
            for block in iter(lambda: stream.read(1024 * 1024), b""):
                digest.update(block)
        if digest.hexdigest() != expected["sha256"]:
            raise RuntimeError(f"Arquivo de backup divergente: {name}")
    with zipfile.ZipFile(backup / "source-and-config.zip") as archive:
        if archive.testzip() is not None:
            raise RuntimeError("ZIP do backup corrompido.")


def verify_database_identity(backup: Path) -> dict:
    saved = json.loads((backup / f"{DB}.json").read_text(encoding="utf-8"))[0]
    current = json.loads(run(["docker", "inspect", DB], backup, "database-identity-current"))[0]
    if current["Id"] != saved["Id"] or current["NetworkSettings"]["Ports"] != saved["NetworkSettings"]["Ports"]:
        raise RuntimeError("Identidade/publicação do banco mudou após o backup; aplicação interrompida.")
    return current


def ef(backup: Path, connection: str, token: str, label: str) -> None:
    child_env = os.environ.copy()
    child_env["ConnectionStrings__DefaultConnection"] = connection
    child_env["AppSettings__Token"] = token
    tool = Path(os.environ["USERPROFILE"]) / ".dotnet/tools/dotnet-ef.exe"
    run([str(tool), "database", "update", EXPECTED, "--project",
         str(ROOT / "PhishGuard.Backend"), "--no-build"], backup, label, env=child_env)


def conn(settings: dict, password: str, host: str, port: int) -> str:
    def quote(value: str) -> str:
        return '"' + value.replace('"', '""') + '"'
    return f"Host={host};Port={port};Database={quote(settings['database'])};Username={quote(settings['user'])};Password={quote(password)};Include Error Detail=false"


def prepare() -> Path:
    parent = ROOT / ".backups"
    parent.mkdir(exist_ok=True)
    sid = next(csv.reader([subprocess.check_output(["whoami", "/user", "/fo", "csv", "/nh"], text=True).strip()]))[1]
    acl = subprocess.run(["icacls", str(parent), "/inheritance:r", "/grant:r",
                          f"*{sid}:(OI)(CI)F", "*S-1-5-18:(OI)(CI)F"], capture_output=True)
    if acl.returncode:
        raise RuntimeError("Não foi possível proteger a pasta de backup.")
    # Windows supplies the Sao Paulo rules without requiring a Python tzdata package.
    stamp = subprocess.check_output(["powershell", "-NoProfile", "-Command",
        "[TimeZoneInfo]::ConvertTimeFromUtc([datetime]::UtcNow, [TimeZoneInfo]::FindSystemTimeZoneById('E. South America Standard Time')).ToString('yyyyMMdd-HHmmss')"], text=True).strip()
    backup = parent / f"mvp-before-migration-{stamp}-{secrets.token_hex(3)}"
    backup.mkdir()
    manifest = {"createdAtUtc": datetime.now(timezone.utc).isoformat(), "status": "preparing"}
    save_manifest(backup, manifest)
    db, api, front = (inspect(name, backup) for name in (DB, API, FRONT))
    db_env = environment(db)
    settings = {"database": db_env["POSTGRES_DB"], "user": db_env["POSTGRES_USER"],
                "port": int(db_env.get("POSTGRES_PORT", "5433"))}
    if settings["port"] != 5433:
        raise RuntimeError("Porta do banco diferente da instalação revisada; interrompido para conferir configuração.")
    effective = run(["docker", "compose", "config", "--format", "json"], backup, "compose-effective")
    (backup / "compose-effective.json").write_text(effective, encoding="utf-8")
    desired = json.loads(effective)["services"]["backend"]["environment"]
    actual = environment(api)
    for key in ("ConnectionStrings__DefaultConnection", "AppSettings__Token",
                "AppSettings__PublicApiBaseUrl", "AppSettings__PublicAppBaseUrl"):
        if actual.get(key) != desired.get(key):
            raise RuntimeError(f"Configuração Docker diverge do compose na chave {key}; conferir antes de recriar serviços.")
    source_count = 0
    print("Copiando código e configurações para backup privado...", flush=True)
    with zipfile.ZipFile(backup / "source-and-config.zip", "w", zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
        for folder, dirs, files in os.walk(ROOT):
            dirs[:] = [d for d in dirs if d not in SKIP and not (Path(folder) / d).is_symlink()]
            for name in files:
                path = Path(folder) / name
                if path.is_symlink() or not path.resolve().is_relative_to(ROOT):
                    continue
                archive.write(path, path.relative_to(ROOT))
                source_count += 1
    run(["git", "bundle", "create", str(backup / "git-history.bundle"), "--all"], backup, "git-bundle")
    run(["git", "bundle", "verify", str(backup / "git-history.bundle")], backup, "git-bundle-verify")
    run(["docker", "cp", f"{API}:/app/DataProtection-Keys", str(backup / "runtime-data-protection")], backup, "key-volume-backup")
    print("Preservando as imagens Docker atualmente instaladas...", flush=True)
    run(["docker", "image", "save", "-o", str(backup / "deployed-images.tar"), api["Image"], front["Image"]], backup, "docker-images-backup")
    print("Pausando a API e copiando o banco consistente...", flush=True)
    run(["docker", "stop", "--time", "30", API], backup, "stop-api")
    ensure_stopped(backup)
    try:
        before = fingerprint(DB, settings, backup, "data-before")
        remote_dump = f"/tmp/phishguard-recovery-{secrets.token_hex(12)}.dump"
        run(["docker", "exec", DB, "pg_dump", "-p", str(settings["port"]), "-U", settings["user"],
             "-d", settings["database"], "-Fc", "-f", remote_dump], backup, "pg-dump")
        run(["docker", "cp", f"{DB}:{remote_dump}", str(backup / "database.dump")], backup, "dump-copy")
        run(["docker", "exec", DB, "rm", "--", remote_dump], backup, "dump-temp-cleanup")
        if not (backup / "database.dump").stat().st_size:
            raise RuntimeError("Backup do banco vazio.")
        if before != fingerprint(DB, settings, backup, "data-after-dump"):
            raise RuntimeError("O banco mudou durante a cópia; migração interrompida.")
        history = psql(DB, settings["user"], settings["database"], settings["port"],
                      'SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";', backup, "migrations-before").splitlines()
        hashes = {}
        for name in ("database.dump", "source-and-config.zip", "git-history.bundle", "deployed-images.tar"):
            digest = hashlib.sha256()
            with (backup / name).open("rb") as stream:
                for block in iter(lambda: stream.read(1024 * 1024), b""):
                    digest.update(block)
            hashes[name] = {"sha256": digest.hexdigest(), "bytes": (backup / name).stat().st_size}
        manifest.update(status="backed-up", settings=settings, postgresImage=db["Image"],
                        backendImage=api["Image"], frontendImage=front["Image"],
                        sourceFiles=source_count, hashes=hashes, originalMigrations=history)
        save_manifest(backup, manifest)
        print(json.dumps({"backupDirectory": str(backup), "status": manifest["status"]}), flush=True)
        return backup
    except Exception:
        # The database has not been migrated at this point; restoring the existing API is safe.
        run(["docker", "start", API], backup, "resume-before-upgrade-failure")
        raise


def verify(backup: Path, manifest: dict) -> None:
    ensure_stopped(backup)
    verify_backup_files(backup, manifest)
    verify_database_identity(backup)
    settings = manifest["settings"]
    if fingerprint(DB, settings, backup, "live-before-verify") != (backup / "data-before.log").read_text(encoding="utf-8").strip():
        raise RuntimeError("Dados mudaram após o backup; faça uma nova cópia.")
    audit(DB, settings, backup, "audit-live-before")
    temp = "phishguard-recovery-check-" + secrets.token_hex(8)
    password = secrets.token_hex(32)
    with socket.socket() as sock:
        sock.bind(("127.0.0.1", 0))
        host_port = sock.getsockname()[1]
    created = False
    try:
        print("Restaurando o dump em PostgreSQL isolado...", flush=True)
        run(["docker", "run", "-d", "--name", temp, "-e", f"POSTGRES_PASSWORD={password}",
             "-e", f"POSTGRES_USER={settings['user']}", "-e", "POSTGRES_DB=postgres",
             "-p", f"127.0.0.1:{host_port}:5433", manifest["postgresImage"], "-p", "5433"], backup, "restore-container-create")
        created = True
        for attempt in range(60):
            check = subprocess.run(["docker", "exec", temp, "pg_isready", "-p", "5433", "-U", settings["user"]], capture_output=True)
            if check.returncode == 0:
                break
            time.sleep(1)
        else:
            raise RuntimeError("PostgreSQL de restauração indisponível.")
        run(["docker", "cp", str(backup / "database.dump"), f"{temp}:/tmp/recovery.dump"], backup, "restore-copy")
        run(["docker", "exec", temp, "pg_restore", "-p", "5433", "-U", settings["user"],
             "-d", "postgres", "--create", "--exit-on-error", "/tmp/recovery.dump"], backup, "restore-database")
        baseline = (backup / "data-before.log").read_text(encoding="utf-8").strip()
        if fingerprint(temp, settings, backup, "data-restored") != baseline:
            raise RuntimeError("A restauração não reproduziu os registros do backup.")
        audit(temp, settings, backup, "audit-restored-before")
        print("Restauração conferida; testando as migrations na cópia...", flush=True)
        token = environment(json.loads((backup / f"{API}.json").read_text(encoding="utf-8"))[0])["AppSettings__Token"]
        run(["dotnet", "build", str(ROOT / "PhishGuard.Backend/PhishGuard.Backend.csproj"), "--no-restore"], backup, "migration-build")
        ef(backup, conn(settings, password, "127.0.0.1", host_port), token, "migration-restored")
        if fingerprint(temp, settings, backup, "data-restored-upgraded") != baseline:
            raise RuntimeError("Upgrade da cópia alterou registros originais.")
        audit(temp, settings, backup, "audit-restored-after")
        manifest.update(status="restore-and-upgrade-verified", restoreVerifiedAtUtc=datetime.now(timezone.utc).isoformat())
        save_manifest(backup, manifest)
        print("Backup restaurável e upgrade da cópia aprovados.", flush=True)
    finally:
        if created:
            run(["docker", "rm", "-f", "-v", temp], backup, "restore-container-cleanup")


def apply(backup: Path, manifest: dict) -> None:
    if manifest["status"] != "restore-and-upgrade-verified":
        raise RuntimeError("Aplicação exige restauração e upgrade da cópia aprovados.")
    ensure_stopped(backup)
    verify_backup_files(backup, manifest)
    db_info = verify_database_identity(backup)
    settings = manifest["settings"]
    baseline = (backup / "data-before.log").read_text(encoding="utf-8").strip()
    if fingerprint(DB, settings, backup, "live-before-apply") != baseline:
        raise RuntimeError("O banco mudou após o backup. Aplicação interrompida.")
    audit(DB, settings, backup, "audit-live-final")
    print("Aplicando as migrations no banco local...", flush=True)
    api_info = json.loads((backup / f"{API}.json").read_text(encoding="utf-8"))[0]
    db_env = environment(db_info)
    bindings = db_info["NetworkSettings"]["Ports"]["5433/tcp"]
    if len(bindings) != 1 or bindings[0]["HostIp"] != "127.0.0.1":
        raise RuntimeError("Publicação do PostgreSQL mudou; conferir alvo antes de aplicar.")
    manifest["status"] = "applying"
    save_manifest(backup, manifest)
    ef(backup, conn(settings, db_env["POSTGRES_PASSWORD"], "127.0.0.1", int(bindings[0]["HostPort"])),
       environment(api_info)["AppSettings__Token"], "migration-live")
    if fingerprint(DB, settings, backup, "data-live-upgraded") != baseline:
        raise RuntimeError("Registros originais divergiram após upgrade; API permanece parada para revisão.")
    audit(DB, settings, backup, "audit-live-after")
    history = psql(DB, settings["user"], settings["database"], settings["port"],
                  'SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";', backup, "migrations-after").splitlines()
    if not history or history[-1] != EXPECTED:
        raise RuntimeError("Migration final não confirmada no banco.")
    manifest.update(status="migrated", appliedMigrations=[m for m in history if m not in manifest["originalMigrations"]],
                    appliedAtUtc=datetime.now(timezone.utc).isoformat())
    save_manifest(backup, manifest)
    print("Migrations aplicadas; registros originais preservados.", flush=True)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("phase", choices=["prepare", "verify", "apply"])
    parser.add_argument("--backup")
    args = parser.parse_args()
    if args.phase == "prepare":
        prepare()
        return
    if not args.backup:
        raise RuntimeError("Informe a pasta privada de backup.")
    backup = Path(args.backup).resolve()
    if not backup.is_relative_to((ROOT / ".backups").resolve()):
        raise RuntimeError("Backup fora da pasta de manutenção autorizada.")
    manifest = json.loads((backup / "manifest.json").read_text(encoding="utf-8"))
    {"verify": verify, "apply": apply}[args.phase](backup, manifest)


if __name__ == "__main__":
    try:
        main()
    except Exception as error:
        print(str(error), file=sys.stderr)
        sys.exit(1)
