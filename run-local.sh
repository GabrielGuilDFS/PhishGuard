#!/usr/bin/env bash
# ============================================================================
# Sobe o ambiente de DEV local com hot-reload (equivalente multiplataforma do
# run-local.bat, para colaboradores em Linux/macOS).
#
#   1. Sobe SO o Postgres (servico `db`) via Docker.
#   2. Backend (dotnet watch) e frontend (vite) rodam no host, cada um num terminal.
#
# Pre-requisitos: Docker, .NET SDK 8, Node 20+. E um .env na raiz (cp .env.example .env).
# ============================================================================
set -euo pipefail
cd "$(dirname "$0")"

if [ ! -f .env ]; then
  echo "ERRO: .env nao encontrado. Rode:  cp .env.example .env  e preencha os segredos." >&2
  exit 1
fi

echo "[1/2] Subindo apenas o Postgres (servico db) via Docker..."
docker compose up -d db

echo "[2/2] Suba o backend e o frontend em terminais separados:"
echo "      Terminal A:  cd PhishGuard.Backend  && dotnet watch run"
echo "      Terminal B:  cd PhishGuard.Frontend && npm install && npm run dev"
echo
echo "API:      http://localhost:5000  (Swagger em /swagger)"
echo "Frontend: http://localhost:5173"
