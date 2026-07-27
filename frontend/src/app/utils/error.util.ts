export function extractError(err: any): string {
  const body = err.error;
  if (typeof body === 'string') return body;
  if (body?.message) return body.message;
  if (Array.isArray(body)) return body.map((e: any) => e.description).join('. ');
  return 'Operation failed';
}
