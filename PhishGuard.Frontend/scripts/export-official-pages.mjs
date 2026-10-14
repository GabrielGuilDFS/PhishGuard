import ts from 'typescript';
import { mkdir, readFile, writeFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import path from 'node:path';

// Run from any directory with: node PhishGuard.Frontend/scripts/export-official-pages.mjs
// The exported resources are committed and embedded by the backend; activation
// captures their content so future catalog releases cannot rewrite a campaign.
const frontendRoot = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const resourceRoot = path.resolve(frontendRoot, '../PhishGuard.Backend/Resources');
const moduleUrls = new Map();
async function moduleUrl(file) {
  if (moduleUrls.has(file)) return moduleUrls.get(file);
  const source = file.endsWith(`${path.sep}emailExpiration.ts`)
    // Resolve mockup dates at campaign capture, keeping exports deterministic.
    ? `export const formatarExpiracaoLink = () => '{{DATA_EXPIRACAO}}';
       export const formatarDataAcessoBRT = () => '{{DATA_ACESSO}}';`
    : await readFile(file, 'utf8');
  let js = ts.transpileModule(source, {
    compilerOptions: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext },
  }).outputText;
  for (const match of js.matchAll(/from\s+['"]([^'"]+)['"]/g)) {
    if (!match[1].startsWith('.')) throw new Error(`Unexpected catalog dependency: ${match[1]}`);
    const dependency = path.resolve(path.dirname(file), `${match[1]}.ts`);
    const relative = path.relative(frontendRoot, dependency);
    if (relative.startsWith('..') || path.isAbsolute(relative)) throw new Error('Dependency outside frontend');
    js = js.replace(match[0], `from '${await moduleUrl(dependency)}'`);
  }
  const url = `data:text/javascript;base64,${Buffer.from(js).toString('base64')}`;
  moduleUrls.set(file, url);
  return url;
}
const { landingTemplates } = await import(await moduleUrl(path.join(frontendRoot, 'src/data/landingTemplates.ts')));
const { educationalTemplates } = await import(await moduleUrl(path.join(frontendRoot, 'src/data/educationalTemplates.ts')));
const { feedbackTrainings } = await import(await moduleUrl(path.join(frontendRoot, 'src/data/feedbackTrainings.ts')));

for (const [directory, entries, suffix] of [
  ['OfficialLandings', landingTemplates.map(({ id, html }) => [id, html]), 'html'],
  ['EducationalTemplates', educationalTemplates.map(({ id, html }) => [id, html]), 'html'],
  ['OfficialFeedback', Object.entries(feedbackTrainings).map(([id, config]) => [id, JSON.stringify(config, null, 2)]), 'json'],
]) {
  const outputDirectory = path.join(resourceRoot, directory);
  await mkdir(outputDirectory, { recursive: true });
  for (const [id, contents] of entries) {
    if (!/^[a-z0-9_-]+$/i.test(id)) throw new Error(`Invalid catalog id: ${id}`);
    await writeFile(path.join(outputDirectory, `${id}.${suffix}`), `${contents}\n`, 'utf8');
  }
  process.stdout.write(`${directory}: ${entries.length} resources exported.\n`);
}
