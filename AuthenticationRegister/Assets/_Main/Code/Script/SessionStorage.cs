using UnityEngine;

public static class SessionStorage
{
    private const string TokenKey = "Token";
    private const string UsernameKey = "Username";

    public static void SaveSession(string token, string username)
    {
        PlayerPrefs.SetString(TokenKey, token);
        PlayerPrefs.SetString(UsernameKey, username);
        PlayerPrefs.Save();
    }

    public static string LoadToken()
    {
        return PlayerPrefs.GetString(TokenKey, "");
    }

    public static string LoadUsername()
    {
        return PlayerPrefs.GetString(UsernameKey, "");
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(TokenKey);
        PlayerPrefs.DeleteKey(UsernameKey);
        PlayerPrefs.Save();
    }
}