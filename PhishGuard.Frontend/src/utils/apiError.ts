export async function apiErrorMessage(response: Response, fallback: string): Promise<string> {
  const raw = await response.text();
  try {
    const data: unknown = JSON.parse(raw);
    if (typeof data === 'string') return data;
    if (data && typeof data === 'object' && 'message' in data && typeof data.message === 'string') {
      return data.message;
    }
  } catch {
    if (raw && response.status < 500) return raw;
  }
  return fallback;
}
