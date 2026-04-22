"use client";

import { ImgHTMLAttributes, ReactNode, useState } from "react";

type ResilientImageProps = Omit<ImgHTMLAttributes<HTMLImageElement>, "src"> & {
  src?: string | null;
  fallback?: ReactNode;
};

function getApiOrigin(): string | null {
  const apiUrl = process.env.NEXT_PUBLIC_API_URL;
  if (!apiUrl) {
    return null;
  }

  try {
    return new URL(apiUrl).origin;
  } catch {
    return null;
  }
}

function escapeXml(value: string): string {
  return value
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll("\"", "&quot;")
    .replaceAll("'", "&apos;");
}

function decodePlaceholderText(value: string): string {
  const normalized = value.replace(/\+/g, " ");
  try {
    return decodeURIComponent(normalized);
  } catch {
    return normalized;
  }
}

function buildInlinePlaceholderFromKnownHost(src: string): string | null {
  try {
    const parsed = new URL(src);
    const host = parsed.hostname.toLowerCase();
    if (host !== "via.placeholder.com" && host !== "placehold.co") {
      return null;
    }

    const pathParts = parsed.pathname.split("/").filter(Boolean);
    const sizeToken = pathParts[0] ?? "400";
    const backgroundToken = pathParts[1] ?? "e2e8f0";
    const foregroundToken = pathParts[2] ?? "334155";

    const sizeMatch = sizeToken.match(/^(\d+)(?:x(\d+))?$/i);
    const width = sizeMatch ? Number(sizeMatch[1]) : 400;
    const height = sizeMatch ? Number(sizeMatch[2] ?? sizeMatch[1]) : width;

    const safeWidth = Number.isFinite(width) && width > 0 ? Math.min(width, 1200) : 400;
    const safeHeight = Number.isFinite(height) && height > 0 ? Math.min(height, 1200) : safeWidth;

    const safeBackground = /^[0-9a-fA-F]{3,8}$/.test(backgroundToken)
      ? `#${backgroundToken}`
      : "#e2e8f0";
    const safeForeground = /^[0-9a-fA-F]{3,8}$/.test(foregroundToken)
      ? `#${foregroundToken}`
      : "#334155";

    const placeholderText = decodePlaceholderText(parsed.searchParams.get("text") ?? "No Image").trim() || "No Image";
    const fontSize = Math.max(16, Math.round(Math.min(safeWidth, safeHeight) * 0.08));

    const svg = `<svg xmlns="http://www.w3.org/2000/svg" width="${safeWidth}" height="${safeHeight}" viewBox="0 0 ${safeWidth} ${safeHeight}"><rect width="100%" height="100%" fill="${safeBackground}" /><text x="50%" y="50%" text-anchor="middle" dominant-baseline="middle" fill="${safeForeground}" font-family="Segoe UI, Arial, sans-serif" font-size="${fontSize}">${escapeXml(placeholderText)}</text></svg>`;
    return `data:image/svg+xml;charset=UTF-8,${encodeURIComponent(svg)}`;
  } catch {
    return null;
  }
}

function buildImageCandidates(rawSrc?: string | null): string[] {
  if (!rawSrc) {
    return [];
  }

  const src = rawSrc.trim();
  if (!src) {
    return [];
  }

  const candidates: string[] = [];
  const addCandidate = (candidate?: string | null) => {
    if (!candidate) {
      return;
    }

    const normalized = candidate.trim();
    if (!normalized || candidates.includes(normalized)) {
      return;
    }

    candidates.push(normalized);
  };

  const inlinePlaceholder = buildInlinePlaceholderFromKnownHost(src);
  if (inlinePlaceholder) {
    addCandidate(inlinePlaceholder);
    return candidates;
  }

  addCandidate(src);

  const apiOrigin = getApiOrigin();

  if (src.startsWith("/") && apiOrigin) {
    addCandidate(`${apiOrigin}${src}`);
  }

  if (typeof window !== "undefined" && window.location.protocol === "https:" && src.startsWith("http://")) {
    addCandidate(`https://${src.slice("http://".length)}`);
  }

  const sourceWithProductsPath = src.replace("/uploads/product-images/", "/uploads/products/");
  addCandidate(sourceWithProductsPath);

  const fileNameMatch = src.match(/\/([^/?#]+)(?:[?#].*)?$/);
  const fileName = fileNameMatch?.[1];

  if (fileName && src.includes("/uploads/")) {
    const queryOrHashMatch = src.match(/([?#].*)$/);
    const suffix = queryOrHashMatch?.[1] ?? "";
    const uploadsIndex = src.indexOf("/uploads/");
    const prefix = src.slice(0, uploadsIndex);

    addCandidate(`${prefix}/uploads/${fileName}/${fileName}${suffix}`);
  }

  if (apiOrigin) {
    try {
      const parsed = new URL(src);
      if (parsed.pathname.startsWith("/uploads/")) {
        const suffix = `${parsed.search}${parsed.hash}`;
        const normalizedPath = parsed.pathname.replace(
          "/uploads/product-images/",
          "/uploads/products/",
        );

        addCandidate(`${apiOrigin}${parsed.pathname}${suffix}`);
        addCandidate(`${apiOrigin}${normalizedPath}${suffix}`);

        const parsedFileNameMatch = parsed.pathname.match(/\/([^/]+)$/);
        const parsedFileName = parsedFileNameMatch?.[1];
        if (parsedFileName) {
          addCandidate(`${apiOrigin}/uploads/${parsedFileName}/${parsedFileName}${suffix}`);
        }
      }
    } catch {
      // Ignore invalid URL and keep existing candidates.
    }
  }

  return candidates;
}

export function ResilientImage({
  src,
  fallback = null,
  onError,
  alt = "",
  ...imgProps
}: ResilientImageProps) {
  const candidates = buildImageCandidates(src);
  const normalizedSource = src ?? "";
  const [imageState, setImageState] = useState({
    source: normalizedSource,
    candidateIndex: 0,
  });
  const candidateIndex =
    imageState.source === normalizedSource ? imageState.candidateIndex : 0;

  if (candidates.length === 0 || candidateIndex >= candidates.length) {
    return <>{fallback}</>;
  }

  const currentSrc = candidates[candidateIndex];

  return (
    // eslint-disable-next-line @next/next/no-img-element
    <img
      {...imgProps}
      src={currentSrc}
      alt={alt}
      onError={(event) => {
        setImageState((current) => ({
          source: normalizedSource,
          candidateIndex:
            (current.source === normalizedSource ? current.candidateIndex : 0) + 1,
        }));
        onError?.(event);
      }}
    />
  );
}
