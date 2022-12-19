namespace ChatGPT.Net.Helpers;

public static class CookieHelper
{
    public static string GetSessionToken(string setCookieHeader)
    {
        // Split the Set-Cookie header into individual cookies
        var cookies = setCookieHeader.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        // Find the __Secure-next-auth.session-token cookie
        var sessionTokenCookie = cookies
            .Select(x => x.Trim())
            .FirstOrDefault(x => x.StartsWith("__Secure-next-auth.session-token="));

        if (sessionTokenCookie == null)
        {
            return null;
        }

        // Extract the value of the cookie
        var parts = sessionTokenCookie.Split('=');
        if (parts.Length < 2)
        {
            return null;
        }

        return parts[1];
    }
}