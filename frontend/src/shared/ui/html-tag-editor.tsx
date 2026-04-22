"use client";

import { useRef } from "react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type HtmlTagEditorProps = {
  id: string;
  value: string;
  onChange: (value: string) => void;
};

function wrapSelection(
  source: string,
  start: number,
  end: number,
  before: string,
  after: string,
  fallback: string,
): { nextValue: string; cursorStart: number; cursorEnd: number } {
  const selected = source.slice(start, end);
  const payload = selected || fallback;
  const nextValue = `${source.slice(0, start)}${before}${payload}${after}${source.slice(end)}`;
  const payloadStart = start + before.length;
  const payloadEnd = payloadStart + payload.length;

  return { nextValue, cursorStart: payloadStart, cursorEnd: payloadEnd };
}

export function HtmlTagEditor({ id, value, onChange }: HtmlTagEditorProps) {
  const textareaRef = useRef<HTMLTextAreaElement | null>(null);

  const applyTag = (before: string, after = "", fallback = "text") => {
    const element = textareaRef.current;
    if (!element) {
      onChange(`${value}${before}${fallback}${after}`);
      return;
    }

    const start = element.selectionStart ?? value.length;
    const end = element.selectionEnd ?? value.length;
    const { nextValue, cursorStart, cursorEnd } = wrapSelection(
      value,
      start,
      end,
      before,
      after,
      fallback,
    );

    onChange(nextValue);

    requestAnimationFrame(() => {
      if (!textareaRef.current) {
        return;
      }
      textareaRef.current.focus();
      textareaRef.current.setSelectionRange(cursorStart, cursorEnd);
    });
  };

  const insertLink = () => {
    const href = window.prompt("Enter URL (https://..., mailto:..., tel:...)");
    if (!href) {
      return;
    }

    applyTag(`<a href="${href.trim()}" target="_blank" rel="noopener noreferrer">`, "</a>", "link");
  };

  return (
    <div className="space-y-2">
      <div className="flex flex-wrap gap-2">
        <Button type="button" variant="outline" size="sm" onClick={() => applyTag("<h2>", "</h2>", "Heading")}>
          H2
        </Button>
        <Button type="button" variant="outline" size="sm" onClick={() => applyTag("<p>", "</p>", "Paragraph")}>
          P
        </Button>
        <Button type="button" variant="outline" size="sm" onClick={() => applyTag("<strong>", "</strong>", "Bold")}>
          Bold
        </Button>
        <Button type="button" variant="outline" size="sm" onClick={() => applyTag("<em>", "</em>", "Italic")}>
          Italic
        </Button>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => applyTag("<ul>\n  <li>", "</li>\n</ul>", "Item")}
        >
          List
        </Button>
        <Button type="button" variant="outline" size="sm" onClick={insertLink}>
          Link
        </Button>
        <Button type="button" variant="outline" size="sm" onClick={() => applyTag("<br/>", "", "")}>
          BR
        </Button>
      </div>

      <textarea
        id={id}
        data-slot="textarea"
        ref={textareaRef}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className={cn(
          "flex min-h-64 w-full rounded-lg border border-input bg-transparent px-2.5 py-2 text-base transition-colors outline-none placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 md:text-sm",
        )}
      />

      <p className="text-xs text-muted-foreground">
        Allowed tags: {"<h2>, <h3>, <p>, <strong>, <em>, <ul>, <ol>, <li>, <a>, <br>"}
      </p>
    </div>
  );
}
