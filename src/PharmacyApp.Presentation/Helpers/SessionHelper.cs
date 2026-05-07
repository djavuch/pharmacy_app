﻿using Microsoft.AspNetCore.Http;

namespace PharmacyApp.Presentation.Helpers;

public static class SessionHelper
{
    private const string CartSessionIdKey = "CartSessionId";
    private const string CartSessionOwnerUserIdKey = "CartSessionOwnerUserId";
    private const string CartSessionIdHeader = "X-Cart-Session-Id";
    private static readonly TimeSpan CartLifeTime = TimeSpan.FromDays(30);

    public static string GetOrCreateSessionId(HttpContext httpContext)
    {
        var existing = TryGetSessionId(httpContext);
        if (!string.IsNullOrWhiteSpace(existing))
        {
            EnsureSessionCookie(httpContext, existing);
            return existing;
        }

        var newId = Guid.NewGuid().ToString("N");

        EnsureSessionCookie(httpContext, newId);

        return newId;
    }

    public static string? TryGetSessionId(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue(CartSessionIdKey, out var existing)
            && TryNormalizeSessionId(existing, out var cookieSessionId))
        {
            return cookieSessionId;
        }

        if (httpContext.Request.Headers.TryGetValue(CartSessionIdHeader, out var headerValues)
            && TryNormalizeSessionId(headerValues.FirstOrDefault(), out var headerSessionId))
        {
            return headerSessionId;
        }

        return null;
    }

    public static void ClearSessionId(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(
            CartSessionIdKey,
            CreateCookieOptions(httpContext));

        ClearSessionOwnerUserId(httpContext);
    }

    public static string? TryGetSessionOwnerUserId(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue(CartSessionOwnerUserIdKey, out var existing)
            && !string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        return null;
    }

    public static void SetSessionOwnerUserId(HttpContext httpContext, string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return;

        httpContext.Response.Cookies.Append(
            CartSessionOwnerUserIdKey,
            userId,
            CreateCookieOptions(httpContext));
    }

    public static void ClearSessionOwnerUserId(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(
            CartSessionOwnerUserIdKey,
            CreateCookieOptions(httpContext));
    }

    private static CookieOptions CreateCookieOptions(HttpContext httpContext)
    {
        var isSecure = IsSecureRequest(httpContext);

        return new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax,
            Secure = isSecure,
            Path = "/",
            MaxAge = CartLifeTime
        };
    }

    private static void EnsureSessionCookie(HttpContext httpContext, string sessionId)
    {
        httpContext.Response.Cookies.Append(
            CartSessionIdKey,
            sessionId,
            CreateCookieOptions(httpContext));
    }

    private static bool TryNormalizeSessionId(string? value, out string sessionId)
    {
        sessionId = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!Guid.TryParseExact(value.Trim(), "N", out var parsed) || parsed == Guid.Empty)
            return false;

        sessionId = parsed.ToString("N");
        return true;
    }

    private static bool IsSecureRequest(HttpContext httpContext)
    {
        if (httpContext.Request.IsHttps)
            return true;

        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-Proto", out var forwardedProto))
        {
            return string.Equals(
                forwardedProto.ToString(),
                "https",
                StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }
}
