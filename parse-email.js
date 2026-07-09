#!/usr/bin/env node
'use strict';

/**
 * parse-email.js — Extrator de HTML puro a partir de e-mails brutos (.eml / logs).
 *
 * Objetivo: facilitar a ingestão de novos moldes (armadilhas) do PhishGuard,
 * transformando um e-mail cru — com cabeçalhos de transporte, múltiplas partes
 * MIME e codificação quoted-printable — em um arquivo HTML limpo e renderizável.
 *
 * Uso:
 *   node parse-email.js <entrada.eml> [saida.html]
 *   node parse-email.js < entrada.eml            (lê da stdin, escreve na stdout)
 *
 * Sem dependências externas: usa apenas Node nativo (Buffer para UTF-8 correto).
 */

const fs = require('fs');
const path = require('path');
const http = require('http');
const { exec } = require('child_process');

/**
 * 1) Localiza o boundary do multipart declarado no cabeçalho Content-Type.
 *    Ex.: boundary="----=_Part_1764637_354420470.1754350779832"
 */
function extrairBoundary(raw) {
  const m = raw.match(/boundary="?([^"\r\n;]+)"?/i);
  return m ? m[1].trim() : null;
}

/**
 * 2) Isola a seção MIME cujo Content-Type é text/html, descartando os headers
 *    de transporte e a versão text/plain. Se não houver multipart, retorna o
 *    conteúdo inteiro para as etapas seguintes tratarem.
 */
function isolarParteHtml(raw, boundary) {
  const partes = boundary ? raw.split('--' + boundary) : [raw];
  const parteHtml = partes.find((p) => /Content-Type:\s*text\/html/i.test(p));
  return parteHtml || raw;
}

/**
 * 3) Decodifica Quoted-Printable de forma segura para acentuação (UTF-8):
 *    - remove as quebras de linha "soft" (um '=' no fim da linha);
 *    - converte cada sequência '=XX' no byte correspondente, montando os bytes
 *      em um Buffer e decodificando como UTF-8 no final. Isso garante que pares
 *      multibyte (ex.: '=C3=A7' -> 'ç', '=C3=A3' -> 'ã') fiquem corretos.
 */
function decodificarQuotedPrintable(texto) {
  // Normaliza CRLF para LF para simplificar o tratamento das quebras.
  // Um soft break é um '=' no fim da linha; a continuação vem logo abaixo, então
  // consumimos também uma eventual linha em branco imediatamente seguinte
  // (artefato comum de logs "espaçados") para não partir palavras (ex.: <bod\ny>).
  const semSoftBreaks = texto
    .replace(/\r\n/g, '\n')
    .replace(/=[ \t]*\n\n?/g, '');

  const bytes = [];
  for (let i = 0; i < semSoftBreaks.length; i++) {
    const c = semSoftBreaks[i];
    const par = semSoftBreaks.substr(i + 1, 2);

    if (c === '=' && /^[0-9A-Fa-f]{2}$/.test(par)) {
      bytes.push(parseInt(par, 16));
      i += 2;
    } else {
      // Empurra os bytes UTF-8 do caractere literal (normalmente ASCII).
      const buf = Buffer.from(c, 'utf8');
      for (const b of buf) bytes.push(b);
    }
  }

  return Buffer.from(bytes).toString('utf8');
}

/**
 * 4) Recorta estritamente o documento HTML: do primeiro '<!doctype html>' ou
 *    '<html ...>' até o último '</html>'. Remove qualquer resíduo de sub-header
 *    MIME ou boundary de fechamento que sobre antes/depois.
 */
function recortarHtml(texto) {
  const inicio = texto.search(/<!doctype html|<html[\s>]/i);
  const fimRel = texto.toLowerCase().lastIndexOf('</html>');
  if (inicio === -1 || fimRel === -1) {
    return texto.trim();
  }
  return texto.slice(inicio, fimRel + '</html>'.length);
}

/**
 * Mapeamento de links: troca o href original de TODAS as tags <a> pela tag
 * dinâmica {{URL_ARMADILHA}}. O backend substitui esse placeholder pela URL da
 * landing page falsa no momento do disparo. Atua apenas em âncoras (não mexe em
 * <link>/<img>), preservando os demais atributos e aspas simples ou duplas.
 */
function mapearLinks(html) {
  return html.replace(/<a\b[^>]*>/gi, (tag) =>
    tag.replace(/href\s*=\s*("[^"]*"|'[^']*')/i, 'href="{{URL_ARMADILHA}}"')
  );
}

/**
 * Decodifica "encoded-words" RFC 2047 usados no header Subject, ex.:
 *   =?UTF-8?B?QWx0ZXJhw6fDo28=?=  (Base64)  ->  "Alteração"
 *   =?UTF-8?Q?Ol=C3=A1?=         (Q/QP)    ->  "Olá"
 */
function decodificarEncodedWord(assunto) {
  return assunto
    .replace(/=\?([^?]+)\?([BbQq])\?([^?]*)\?=/g, (_m, _charset, enc, dados) => {
      if (enc.toUpperCase() === 'B') {
        return Buffer.from(dados, 'base64').toString('utf8');
      }
      // Q-encoding: '_' representa espaço; '=XX' é um byte.
      const s = dados.replace(/_/g, ' ');
      const bytes = [];
      for (let i = 0; i < s.length; i++) {
        const par = s.substr(i + 1, 2);
        if (s[i] === '=' && /^[0-9A-Fa-f]{2}$/.test(par)) {
          bytes.push(parseInt(par, 16));
          i += 2;
        } else {
          for (const b of Buffer.from(s[i], 'utf8')) bytes.push(b);
        }
      }
      return Buffer.from(bytes).toString('utf8');
    })
    .replace(/\s+/g, ' ')
    .trim();
}

/**
 * Extrai e decodifica o assunto do e-mail (usado para nomear o template).
 * Trata o "folding" de headers (continuação em linhas iniciadas por espaço/tab).
 */
function extrairAssunto(raw) {
  const m = raw.match(/^Subject:\s*(.*(?:\r?\n[ \t].*)*)/im);
  if (!m) return null;
  const bruto = m[1].replace(/\r?\n[ \t]/g, ' ');
  const assunto = decodificarEncodedWord(bruto);
  return assunto || null;
}

/**
 * Converte um texto em um slug seguro para nome de arquivo (sem acentos/espaços).
 */
function slugify(texto, fallback = 'template') {
  const base = (texto || '')
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '') // remove diacriticos
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
    .slice(0, 60);
  return base || fallback;
}

/**
 * Orquestra o pipeline completo: recebe o e-mail bruto e devolve o HTML limpo.
 */
function extrairHtmlLimpo(raw) {
  const boundary = extrairBoundary(raw);
  const parteHtml = isolarParteHtml(raw, boundary);

  const conteudo = /Content-Transfer-Encoding:\s*quoted-printable/i.test(parteHtml)
    ? decodificarQuotedPrintable(parteHtml)
    : parteHtml;

  const html = recortarHtml(conteudo).trim();
  if (!html) {
    throw new Error('Não foi possível localizar um bloco <html>...</html> no e-mail.');
  }
  return html + '\n';
}

// ------------------------------------------------------------------
// CLI
// ------------------------------------------------------------------

function lerEntrada(caminho) {
  if (caminho) return fs.readFileSync(caminho, 'utf8');
  // Sem argumento: lê da stdin (permite "node parse-email.js < email.eml").
  return fs.readFileSync(0, 'utf8');
}

/**
 * Abre a URL no navegador padrão do sistema via child_process, escolhendo o
 * comando conforme a plataforma (Windows: start, macOS: open, Linux: xdg-open).
 */
function abrirNavegador(url) {
  const comandos = {
    win32: `start "" "${url}"`,
    darwin: `open "${url}"`,
  };
  const comando = comandos[process.platform] || `xdg-open "${url}"`;
  exec(comando, (err) => {
    if (err) {
      console.error(`[PhishGuard] Não foi possível abrir o navegador automaticamente. Acesse: ${url}`);
    }
  });
}

/**
 * Sobe um servidor HTTP nativo servindo o HTML limpo e abre o navegador nele.
 * Procura uma porta livre a partir de `portaInicial` (default 3000), tratando
 * EADDRINUSE ao tentar a próxima porta. Mantém o processo vivo até Ctrl+C.
 */
function iniciarPreview(html, portaInicial = 3000) {
  const server = http.createServer((req, res) => {
    res.writeHead(200, { 'Content-Type': 'text/html; charset=utf-8' });
    res.end(html);
  });

  let porta = portaInicial;

  server.on('error', (err) => {
    if (err.code === 'EADDRINUSE' && porta - portaInicial < 20) {
      porta += 1;
      server.listen(porta);
    } else {
      console.error(`[PhishGuard] Falha ao iniciar o servidor de preview: ${err.message}`);
      process.exit(1);
    }
  });

  server.listen(porta, () => {
    const url = `http://localhost:${porta}`;
    console.error(
      `\n[PhishGuard] Preview do e-mail disponível em ${url} - Pressione Ctrl+C para encerrar\n`
    );
    abrirNavegador(url);
  });
}

function main() {
  // Separa flags (--preview) dos argumentos posicionais (entrada, saída).
  const args = process.argv.slice(2);
  const preview = args.includes('--preview');
  const posicionais = args.filter((a) => !a.startsWith('--'));
  const [entrada, saida] = posicionais;

  let raw;
  try {
    raw = lerEntrada(entrada);
  } catch (err) {
    console.error(`Erro ao ler a entrada: ${err.message}`);
    process.exit(1);
  }

  let html;
  try {
    // Extrai o HTML fiel e o prepara para o ecossistema: mapeia os links para
    // o placeholder {{URL_ARMADILHA}} que o backend substitui no disparo.
    html = mapearLinks(extrairHtmlLimpo(raw));
  } catch (err) {
    console.error(`Falha ao extrair o HTML: ${err.message}`);
    process.exit(1);
  }

  // Modo preview: em vez de (apenas) salvar, sobe um servidor e abre o navegador.
  if (preview) {
    iniciarPreview(html);
    return;
  }

  // Define o destino local: argumento explícito > arquivo ao lado da entrada > stdout.
  const destino =
    saida || (entrada ? entrada.replace(/\.[^.]+$/, '') + '.clean.html' : null);

  if (destino) {
    fs.writeFileSync(destino, html, 'utf8');
    console.error(`HTML limpo salvo em: ${path.resolve(destino)} (${html.length} chars)`);
  } else {
    process.stdout.write(html);
  }

  // Cópia padronizada na pasta central de templates do frontend, nomeada pelo
  // assunto do e-mail (slug), facilitando a leitura pelo seletor React.
  try {
    const slug = slugify(
      extrairAssunto(raw) ||
        (entrada ? path.basename(entrada).replace(/\.[^.]+$/, '') : 'template')
    );
    const templatesDir = path.join(
      __dirname,
      'PhishGuard.Frontend',
      'src',
      'assets',
      'templates'
    );
    fs.mkdirSync(templatesDir, { recursive: true });
    const templatePath = path.join(templatesDir, `${slug}.html`);
    fs.writeFileSync(templatePath, html, 'utf8');
    console.error(`Template centralizado salvo em: ${templatePath}`);
  } catch (err) {
    console.error(`Aviso: não foi possível salvar o template centralizado: ${err.message}`);
  }
}

// Executa apenas quando chamado diretamente (permite reuso via require()).
if (require.main === module) {
  main();
}

module.exports = {
  extrairBoundary,
  isolarParteHtml,
  decodificarQuotedPrintable,
  recortarHtml,
  extrairHtmlLimpo,
  mapearLinks,
  decodificarEncodedWord,
  extrairAssunto,
  slugify,
};
