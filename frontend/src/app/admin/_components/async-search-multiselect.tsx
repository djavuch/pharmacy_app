"use client";

import { useEffect, useRef, useState } from "react";
import { Loader2, Search, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Badge } from "@/components/ui/badge";

export type AsyncSearchOption = {
  id: number;
  label: string;
  description?: string;
};

export type AsyncSearchPage = {
  items: AsyncSearchOption[];
  hasNextPage: boolean;
};

type FetchOptionsArgs = {
  query: string;
  page: number;
  pageSize: number;
  signal: AbortSignal;
};

export function AsyncSearchMultiSelect({
  label,
  placeholder,
  selectedIds,
  onChange,
  fetchOptions,
  loadSelectedOptions,
  minQueryLength = 2,
  pageSize = 20,
  allowEmptyQuery = false,
  disabled = false,
  hint,
}: {
  label: string;
  placeholder: string;
  selectedIds: number[];
  onChange: (nextIds: number[]) => void;
  fetchOptions: (args: FetchOptionsArgs) => Promise<AsyncSearchPage>;
  loadSelectedOptions?: (ids: number[]) => Promise<AsyncSearchOption[]>;
  minQueryLength?: number;
  pageSize?: number;
  allowEmptyQuery?: boolean;
  disabled?: boolean;
  hint?: string;
}) {
  const [query, setQuery] = useState("");
  const [debouncedQuery, setDebouncedQuery] = useState("");
  const [page, setPage] = useState(1);
  const [items, setItems] = useState<AsyncSearchOption[]>([]);
  const [hasNextPage, setHasNextPage] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedById, setSelectedById] = useState<Record<number, AsyncSearchOption>>({});
  const fetchOptionsRef = useRef(fetchOptions);
  const loadSelectedOptionsRef = useRef(loadSelectedOptions);

  useEffect(() => {
    fetchOptionsRef.current = fetchOptions;
  }, [fetchOptions]);

  useEffect(() => {
    loadSelectedOptionsRef.current = loadSelectedOptions;
  }, [loadSelectedOptions]);

  useEffect(() => {
    const timeoutId = setTimeout(() => {
      setDebouncedQuery(query.trim());
    }, 300);

    return () => clearTimeout(timeoutId);
  }, [query]);

  useEffect(() => {
    setPage(1);
    setItems([]);
    setHasNextPage(false);
  }, [debouncedQuery]);

  useEffect(() => {
    const canSearch = allowEmptyQuery || debouncedQuery.length >= minQueryLength;
    if (!canSearch || disabled) {
      setItems([]);
      setHasNextPage(false);
      setLoading(false);
      setError(null);
      return;
    }

    let ignore = false;
    const abortController = new AbortController();

    const fetchPage = async () => {
      setLoading(true);
      setError(null);
      try {
        const result = await fetchOptionsRef.current({
          query: debouncedQuery,
          page,
          pageSize,
          signal: abortController.signal,
        });

        if (ignore) {
          return;
        }

        setItems((current) => {
          const source = page === 1 ? [] : current;
          const merged = [...source];
          const seen = new Set(merged.map((entry) => entry.id));

          for (const item of result.items) {
            if (!seen.has(item.id)) {
              merged.push(item);
              seen.add(item.id);
            }
          }

          return merged;
        });
        setHasNextPage(result.hasNextPage);
        setSelectedById((current) => {
          const next = { ...current };
          for (const item of result.items) {
            if (selectedIds.includes(item.id)) {
              next[item.id] = item;
            }
          }
          return next;
        });
      } catch (err) {
        if (ignore || abortController.signal.aborted) {
          return;
        }

        setItems([]);
        setHasNextPage(false);
        setError(err instanceof Error ? err.message : "Failed to load options.");
      } finally {
        if (!ignore) {
          setLoading(false);
        }
      }
    };

    void fetchPage();

    return () => {
      ignore = true;
      abortController.abort();
    };
  }, [allowEmptyQuery, debouncedQuery, disabled, minQueryLength, page, pageSize, selectedIds]);

  useEffect(() => {
    if (selectedIds.length === 0 || !loadSelectedOptionsRef.current) {
      return;
    }

    const missingIds = selectedIds.filter((id) => !selectedById[id]);
    if (missingIds.length === 0) {
      return;
    }

    let ignore = false;
    const hydrateSelected = async () => {
      try {
        const options = await loadSelectedOptionsRef.current!(missingIds);
        if (ignore) {
          return;
        }

        setSelectedById((current) => {
          const next = { ...current };
          for (const option of options) {
            next[option.id] = option;
          }
          return next;
        });
      } catch {
        // Best effort loading of selected labels.
      }
    };

    void hydrateSelected();

    return () => {
      ignore = true;
    };
  }, [selectedById, selectedIds]);

  const canSearch = allowEmptyQuery || debouncedQuery.length >= minQueryLength;

  const handleAdd = (id: number) => {
    if (selectedIds.includes(id)) {
      return;
    }
    onChange([...selectedIds, id]);
  };

  const handleRemove = (id: number) => {
    onChange(selectedIds.filter((entry) => entry !== id));
  };

  return (
    <div className="space-y-2 rounded-md border p-3">
      <div className="flex items-center justify-between">
        <Label>{label}</Label>
        <span className="text-xs text-muted-foreground">
          Selected: {selectedIds.length}
        </span>
      </div>

      <Input
        placeholder={placeholder}
        value={query}
        onChange={(event) => setQuery(event.target.value)}
        disabled={disabled}
      />

      {hint && (
        <p className="text-xs text-muted-foreground">{hint}</p>
      )}

      {selectedIds.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {selectedIds.map((id) => {
            const option = selectedById[id];
            return (
              <Badge key={`${label}-selected-${id}`} variant="outline" className="gap-1">
                <span>{option?.label ?? `#${id}`}</span>
                <button
                  type="button"
                  onClick={() => handleRemove(id)}
                  className="rounded-sm p-0.5 hover:bg-muted"
                  disabled={disabled}
                  aria-label={`Remove ${option?.label ?? id}`}
                >
                  <X className="h-3 w-3" />
                </button>
              </Badge>
            );
          })}
        </div>
      )}

      {!canSearch && !disabled && (
        <p className="text-xs text-muted-foreground">
          Type at least {minQueryLength} characters to search.
        </p>
      )}

      {canSearch && !disabled && (
        <div className="space-y-2 rounded-md border p-2">
          {error && (
            <div className="rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-xs text-rose-700">
              {error}
            </div>
          )}

          {loading && page === 1 ? (
            <div className="flex items-center gap-2 text-sm text-muted-foreground">
              <Loader2 className="h-4 w-4 animate-spin" />
              Loading options...
            </div>
          ) : items.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No options found.
            </p>
          ) : (
            <div className="max-h-48 space-y-2 overflow-y-auto">
              {items.map((item) => {
                const selected = selectedIds.includes(item.id);
                return (
                  <div
                    key={`${label}-option-${item.id}`}
                    className="flex items-start justify-between gap-3 rounded-md border p-2"
                  >
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium">{item.label}</p>
                      {item.description && (
                        <p className="truncate text-xs text-muted-foreground">{item.description}</p>
                      )}
                    </div>
                    <Button
                      type="button"
                      size="sm"
                      variant={selected ? "secondary" : "outline"}
                      onClick={() => {
                        if (selected) {
                          handleRemove(item.id);
                          return;
                        }
                        handleAdd(item.id);
                      }}
                    >
                      {selected ? "Remove" : "Add"}
                    </Button>
                  </div>
                );
              })}
            </div>
          )}

          <div className="flex items-center justify-between">
            {loading && page > 1 ? (
              <span className="inline-flex items-center gap-2 text-xs text-muted-foreground">
                <Loader2 className="h-3.5 w-3.5 animate-spin" />
                Loading more...
              </span>
            ) : (
              <span className="text-xs text-muted-foreground">
                {items.length > 0 ? `${items.length} loaded` : ""}
              </span>
            )}

            {hasNextPage && (
              <Button
                type="button"
                size="sm"
                variant="outline"
                onClick={() => setPage((current) => current + 1)}
                disabled={loading}
              >
                <Search className="mr-1 h-3.5 w-3.5" />
                Load more
              </Button>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
