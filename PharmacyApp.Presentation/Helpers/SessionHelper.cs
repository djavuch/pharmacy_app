using Microsoft.AspNetCore.Http;

namespace PharmacyApp.Presentation.Helpers;

public static class SessionHelper
{
    private const string SessionIdKey = "CartSessionId";

    public static string GetSessionId(HttpContext httpContext)
    {
        var sessionId = httpContext.Session.GetString(SessionIdKey);

        if(string.IsNullOrEmpty(sessionId))
        {
            sessionId = Guid.NewGuid().ToString();
            httpContext.Session.SetString(SessionIdKey, sessionId);
        }

        return sessionId;
    }

    public static void ClearSessionId(HttpContext httpContext)
    {
        httpContext.Session.Remove(SessionIdKey);
        httpContext.Session.Clear();
    }
}
