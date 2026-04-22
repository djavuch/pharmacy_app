import { useEffect, useMemo, useState } from "react";
import { Save } from "lucide-react";
import { adminContentPageApi } from "@/shared/api";
import type { AdminContentPageDto } from "@/shared/types";
import { TabsContent } from "@/components/ui/tabs";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { HtmlTagEditor } from "@/shared/ui/html-tag-editor";
import { sanitizeRichHtml } from "@/shared/lib/sanitize-rich-html";

const managedPageLabels: Record<string, string> = {
  contacts: "Contacts",
  "license-agreement": "License Agreement",
  about: "About Company",
};

function formatDate(value: string): string {
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) {
    return "-";
  }

  return parsed.toLocaleString();
}

export function ContentPagesTab() {
  const [pages, setPages] = useState<AdminContentPageDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [selectedSlug, setSelectedSlug] = useState<string | null>(null);
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [isPublished, setIsPublished] = useState(true);

  const selectedPage = useMemo(
    () => pages.find((page) => page.slug === selectedSlug) ?? null,
    [pages, selectedSlug],
  );

  useEffect(() => {
    const fetchPages = async () => {
      setLoading(true);
      setError(null);

      try {
        const result = await adminContentPageApi.getAll();
        setPages(result);

        setSelectedSlug((currentSlug) => {
          if (result.length === 0) {
            return null;
          }

          if (currentSlug && result.some((page) => page.slug === currentSlug)) {
            return currentSlug;
          }

          return result[0].slug;
        });
      } catch (err) {
        setPages([]);
        setSelectedSlug(null);
        setError(err instanceof Error ? err.message : "Failed to load content pages");
      } finally {
        setLoading(false);
      }
    };

    void fetchPages();
  }, []);

  useEffect(() => {
    if (!selectedPage) {
      setTitle("");
      setContent("");
      setIsPublished(true);
      return;
    }

    setTitle(selectedPage.title);
    setContent(selectedPage.content);
    setIsPublished(selectedPage.isPublished);
  }, [selectedPage]);

  const handleSave = async () => {
    if (!selectedPage) {
      return;
    }

    setSaving(true);
    setError(null);
    setSuccessMessage(null);

    try {
      const updated = await adminContentPageApi.update(selectedPage.slug, {
        title,
        content,
        isPublished,
      });

      setPages((current) =>
        current.map((page) => (page.slug === updated.slug ? updated : page)),
      );
      setSuccessMessage(`Page "${updated.slug}" has been updated.`);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to update page");
    } finally {
      setSaving(false);
    }
  };

  return (
    <TabsContent value="contentPages">
      <Card>
        <CardHeader>
          <div>
            <CardTitle>Content Pages</CardTitle>
            <CardDescription>
              Edit fixed storefront pages: contacts, license agreement, and about company.
            </CardDescription>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          {error && (
            <div className="rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700">
              {error}
            </div>
          )}

          {successMessage && (
            <div className="rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">
              {successMessage}
            </div>
          )}

          {loading ? (
            <p className="py-8 text-center text-muted-foreground">Loading pages...</p>
          ) : (
            <>
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Slug</TableHead>
                    <TableHead>Title</TableHead>
                    <TableHead>Status</TableHead>
                    <TableHead>Updated</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {pages.map((page) => (
                    <TableRow
                      key={page.id}
                      className={page.slug === selectedSlug ? "cursor-pointer bg-muted/30" : "cursor-pointer"}
                      onClick={() => setSelectedSlug(page.slug)}
                    >
                      <TableCell className="font-medium">
                        {managedPageLabels[page.slug] ?? page.slug}
                      </TableCell>
                      <TableCell>{page.title}</TableCell>
                      <TableCell>
                        <Badge variant={page.isPublished ? "default" : "secondary"}>
                          {page.isPublished ? "Published" : "Draft"}
                        </Badge>
                      </TableCell>
                      <TableCell>{formatDate(page.updatedAt)}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {pages.length === 0 && (
                <p className="py-8 text-center text-muted-foreground">
                  No managed pages found.
                </p>
              )}

              {selectedPage && (
                <div className="space-y-4 rounded-lg border p-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="text-sm font-semibold">
                        Editing: <span className="font-mono">{managedPageLabels[selectedPage.slug] ?? selectedPage.slug}</span>
                      </h3>
                      <p className="text-xs text-muted-foreground">
                        Last updated: {formatDate(selectedPage.updatedAt)}
                      </p>
                    </div>
                    <Button onClick={handleSave} disabled={saving}>
                      <Save className="mr-2 h-4 w-4" />
                      {saving ? "Saving..." : "Save"}
                    </Button>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="content-page-title">Title</Label>
                    <Input
                      id="content-page-title"
                      value={title}
                      onChange={(event) => setTitle(event.target.value)}
                    />
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="content-page-body">Content</Label>
                    <HtmlTagEditor
                      id="content-page-body"
                      value={content}
                      onChange={setContent}
                    />
                  </div>

                  <label className="inline-flex items-center gap-2 text-sm">
                    <input
                      type="checkbox"
                      checked={isPublished}
                      onChange={(event) => setIsPublished(event.target.checked)}
                    />
                    Published
                  </label>

                  <div className="space-y-2">
                    <Label>Preview</Label>
                    <div
                      className="rounded-md border bg-muted/30 p-3 text-sm whitespace-pre-wrap [&_a]:underline [&_h2]:text-lg [&_h2]:font-semibold [&_h3]:font-semibold [&_ol]:list-decimal [&_ol]:pl-5 [&_p]:mb-2 [&_ul]:list-disc [&_ul]:pl-5"
                      dangerouslySetInnerHTML={{ __html: sanitizeRichHtml(content) }}
                    />
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </TabsContent>
  );
}
