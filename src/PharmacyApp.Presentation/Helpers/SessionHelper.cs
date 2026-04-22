﻿using Microsoft.AspNetCore.Http;

namespace PharmacyApp.Presentation.Helpers;

public static class SessionHelper
{
    private const string CartSessionIdKey = "CartSessionId";
    private const string CartSessionOwnerUserIdKey = "CartSessionOwnerUserId";
    private static readonly TimeSpan CartLifeTime = TimeSpan.FromDays(30);

    public static string GetOrCreateSessionId(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue(CartSessionIdKey, out var existing)
            && !string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var newId = Guid.NewGuid().ToString("N");

        httpContext.Response.Cookies.Append(
            CartSessionIdKey,
            newId,
            CreateCookieOptions(httpContext));

        return newId;
    }

    public static string? TryGetSessionId(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue(CartSessionIdKey, out var existing)
            && !string.IsNullOrWhiteSpace(existing))
        {
            return existing;
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
