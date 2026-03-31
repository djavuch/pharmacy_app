using Microsoft.AspNetCore.Http;

namespace PharmacyApp.Presentation.Helpers;

public static class SessionHelper
{
    private const string CartSessionIdKey = "CartSessionId";
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
            CreateCookieOptions());
        
        return newId;
    }
    
    // For login/register
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
        httpContext.Response.Cookies.Delete(CartSessionIdKey, CreateDeleleCookieOptions());
    }

    private static CookieOptions CreateCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.None,
            Secure = true,
            Path = "/",
            MaxAge = CartLifeTime
        };
    }

    private static CookieOptions CreateDeleleCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.None,
            Secure = true,
            Path = "/",
        };
    }
}