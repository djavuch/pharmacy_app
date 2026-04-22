export function parsePositiveIntParam(value: string | null, fallback: number): number {
  if (!value) {
    return fallback;
  }

  const parsed = Number.parseInt(value, 10);
  return Number.isFinite(parsed) && parsed > 0 ? parsed : fallback;
}

export function parseEnumParam<T extends string>(
  value: string | null,
  allowedValues: readonly T[],
  fallback: T,
): T {
  if (!value) {
    return fallback;
  }

  return (allowedValues as readonly string[]).includes(value)
    ? (value as T)
    : fallback;
}

export function getCurrentUrl(pathname: string, currentSearchParams: { toString(): string }): string {
  const currentQuery = currentSearchParams.toString();
  return currentQuery ? `${pathname}?${currentQuery}` : pathname;
}

export function buildUrlWithQuery(
  pathname: string,
  currentSearchParams: { toString(): string },
  updates: Record<string, string | number | null | undefined>,
): string {
  const nextParams = new URLSearchParams(currentSearchParams.toString());

  for (const [key, value] of Object.entries(updates)) {
    if (value === undefined || value === null || value === "") {
      nextParams.delete(key);
      continue;
    }

    nextParams.set(key, String(value));
  }

  const nextQuery = nextParams.toString();
  return nextQuery ? `${pathname}?${nextQuery}` : pathname;
}
