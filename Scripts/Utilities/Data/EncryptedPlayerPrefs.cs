using UnityEngine;
using VGameFoundation.Scripts.Utilities.Data;

public static class EncryptedPlayerPrefs
{
    private const string ENCRYPTION_KEY = "cat@gamestudio";

    private static readonly AesTextCipher Cipher = new(ENCRYPTION_KEY);

    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, Cipher.Encrypt(value));
    }

    public static string GetString(string key)
    {
        var storedValue = PlayerPrefs.GetString(key);
        return Cipher.TryDecrypt(storedValue, out var value) ? value : string.Empty;
    }

    public static void SetInt(string key, int value)
    {
        SetString(key, value.ToString());
    }

    public static int GetInt(string key)
    {
        return int.TryParse(GetString(key), out var result) ? result : 0;
    }

    public static void SetBool(string key, bool value)
    {
        SetString(key, value.ToString());
    }

    public static bool GetBool(string key)
    {
        return bool.TryParse(GetString(key), out var result) && result;
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}
