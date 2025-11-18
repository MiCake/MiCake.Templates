namespace StandardWeb.Common.Helpers;

using System.Security.Cryptography;
using System.Text;
using System.IO;

public static class EncryptionHelper
{
    public static (string hash, string? salt) HashContent(string content, bool useSalt = true)
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentNullException(nameof(content), "Content cannot be null or empty.");
        }

        // use BCrypt to generate a hash
        var saltValue = useSalt ? BCrypt.Net.BCrypt.GenerateSalt() : null;
        var hash = saltValue == null ? BCrypt.Net.BCrypt.HashPassword(content) : BCrypt.Net.BCrypt.HashPassword(content, saltValue);
        return (hash, saltValue);
    }

    public static bool VerifyHash(string content, string hash, string salt = "")
    {
        if (string.IsNullOrEmpty(content))
        {
            throw new ArgumentNullException(nameof(content), "Content cannot be null or empty.");
        }

        if (string.IsNullOrEmpty(hash))
        {
            throw new ArgumentNullException(nameof(hash), "Hash cannot be null or empty.");
        }

        // use BCrypt to verify the hash
        return BCrypt.Net.BCrypt.Verify(content, hash);
    }

    public static string HideSecret(string secret, int visibleChars = 4)
    {
        if (string.IsNullOrEmpty(secret) || visibleChars < 0)
        {
            return string.Empty;
        }

        if (secret.Length <= visibleChars * 2)
        {
            return new string('*', secret.Length);
        }

        var start = secret[..visibleChars];
        var end = secret[^visibleChars..];
        var hiddenPart = new string('*', secret.Length - visibleChars * 2);
        return $"{start}{hiddenPart}{end}";
    }

    public static string AESEncrypt(string plainText, string encryptionKey)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
        aes.IV = new byte[16];

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
        using var sw = new StreamWriter(cs);
        sw.Write(plainText);
        sw.Close();
        return Convert.ToBase64String(ms.ToArray());
    }

    public static string AESDecrypt(string cipherText, string encryptionKey)
    {
        using var aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(encryptionKey);
        aes.IV = new byte[16];

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}
