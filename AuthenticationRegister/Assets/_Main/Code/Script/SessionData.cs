public static class SessionData
{
    public static string Token;
    public static User CurrentUser;

    public static bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(Token);
    }

    public static void Clear()
    {
        Token = null;
        CurrentUser = null;
    }
}