"use client";

import { useEffect, useState } from "react";
import { contentPageApi } from "@/shared/api";
import type { ContentPageDto } from "@/shared/types";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { sanitizeRichHtml } from "@/shared/lib/sanitize-rich-html";

export default function ContactsPage() {
  const [page, setPage] = useState<ContentPageDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const result = await contentPageApi.getBySlug("contacts");
        setPage(result);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load contacts page");
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, []);

  return (
    <div className="container mx-auto px-4 py-12">
      <Card className="mx-auto max-w-3xl rounded-[2rem] border-border/70 bg-card/88">
        <CardHeader>
          <CardTitle className="text-3xl font-semibold tracking-tight">
            {page?.title ?? "Contacts"}
          </CardTitle>
        </CardHeader>
        <CardContent className="space-y-3 text-muted-foreground">
          {loading && <p>Loading contacts...</p>}
          {!loading && error && (
            <p>
              Contacts are temporarily unavailable. Please try again later.
            </p>
          )}
          {!loading && !error && page && (
            <div
              className="whitespace-pre-wrap [&_a]:underline [&_h2]:mb-2 [&_h2]:text-lg [&_h2]:font-semibold [&_h3]:mb-2 [&_h3]:font-semibold [&_ol]:mb-2 [&_ol]:list-decimal [&_ol]:pl-5 [&_p]:mb-2 [&_ul]:mb-2 [&_ul]:list-disc [&_ul]:pl-5"
              dangerouslySetInnerHTML={{ __html: sanitizeRichHtml(page.content) }}
            />
          )}
        </CardContent>
      </Card>
    </div>
  );
}
