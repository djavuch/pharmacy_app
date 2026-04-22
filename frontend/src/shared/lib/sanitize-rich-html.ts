const ALLOWED_TAGS = new Set([
  "h2",
  "h3",
  "p",
  "strong",
  "em",
  "ul",
  "ol",
  "li",
  "a",
  "br",
]);

const ALLOWED_ATTRS: Record<string, Set<string>> = {
  a: new Set(["href", "target", "rel"]),
};

function escapeHtml(value: string): string {
  return value
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
}

function isSafeHref(href: string): boolean {
  const normalized = href.trim().toLowerCase();
  return (
    normalized.startsWith("http://")
    || normalized.startsWith("https://")
    || normalized.startsWith("mailto:")
    || normalized.startsWith("tel:")
    || normalized.startsWith("/")
  );
}

function sanitizeNode(node: Node): void {
  for (const child of Array.from(node.childNodes)) {
    if (child.nodeType !== Node.ELEMENT_NODE) {
      continue;
    }

    const element = child as HTMLElement;
    const tag = element.tagName.toLowerCase();

    if (!ALLOWED_TAGS.has(tag)) {
      const replacement = document.createTextNode(element.textContent ?? "");
      element.replaceWith(replacement);
      continue;
    }

    const allowedAttrs = ALLOWED_ATTRS[tag] ?? new Set<string>();
    for (const attribute of Array.from(element.attributes)) {
      if (!allowedAttrs.has(attribute.name.toLowerCase())) {
        element.removeAttribute(attribute.name);
      }
    }

    if (tag === "a") {
      const href = element.getAttribute("href") ?? "";
      if (!isSafeHref(href)) {
        element.removeAttribute("href");
      }

      if (element.getAttribute("target") === "_blank") {
        element.setAttribute("rel", "noopener noreferrer");
      }
    }

    sanitizeNode(element);
  }
}

export function sanitizeRichHtml(content: string): string {
  if (!content.trim()) {
    return "";
  }

  if (typeof window === "undefined") {
    return escapeHtml(content).replaceAll("\n", "<br/>");
  }

  const parser = new DOMParser();
  const documentFragment = parser.parseFromString(`<div>${content}</div>`, "text/html");
  const root = documentFragment.body.firstElementChild as HTMLElement | null;

  if (!root) {
    return "";
  }

  sanitizeNode(root);
  return root.innerHTML;
}
