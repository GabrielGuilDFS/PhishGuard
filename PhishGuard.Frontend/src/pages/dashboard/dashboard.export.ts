import type { DashboardPeriod } from './dashboard.types';

export type DashboardExportFormat = 'pdf' | 'csv';

export function buildDashboardQuery(period: DashboardPeriod, department: string): URLSearchParams {
  const params = new URLSearchParams({ period });
  if (department.trim()) params.set('department', department.trim());
  return params;
}

export function parseDownloadFilename(
  contentDisposition: string | null,
  fallback: string,
): string {
  if (!contentDisposition) return fallback;

  const encoded = contentDisposition.match(/filename\*\s*=\s*UTF-8''([^;]+)/i)?.[1];
  if (encoded) {
    try {
      return sanitizeFilename(decodeURIComponent(encoded.trim()));
    } catch {
      // Header malformado: tenta o filename ASCII antes do fallback.
    }
  }

  const quoted = contentDisposition.match(/filename\s*=\s*"([^"]+)"/i)?.[1];
  const plain = contentDisposition.match(/filename\s*=\s*([^;]+)/i)?.[1];
  return sanitizeFilename((quoted ?? plain ?? fallback).trim());
}

export function downloadDashboardBlob(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = sanitizeFilename(filename);
  anchor.style.display = 'none';
  document.body.appendChild(anchor);
  try {
    anchor.click();
  } finally {
    anchor.remove();
    URL.revokeObjectURL(url);
  }
}

function sanitizeFilename(value: string): string {
  const sanitized = Array.from(value, (character) => {
    const codePoint = character.codePointAt(0) ?? 0;
    return character === '\\' || character === '/' || codePoint <= 31 || codePoint === 127
      ? '-'
      : character;
  }).join('').trim();
  return sanitized || 'phishguard-dashboard';
}
