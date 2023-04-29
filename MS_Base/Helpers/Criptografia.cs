using System;
using System.Security.Cryptography;
using System.Text;

namespace MS_Base.Helpers;

public static class Criptografia
{
    //Must initialize with yout own keys
    public static string strKey { get; set; }
    public static string strDBKey { get; set; }

    public static string Encrypt(string message, string key)
    {
        string encryptedMsg = "";

        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.ASCII.GetBytes(key);
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var buffer = Encoding.ASCII.GetBytes(message);

            encryptedMsg = Convert.ToBase64String(encryptor.TransformFinalBlock(buffer, 0, buffer.Length));
        }

        return encryptedMsg;
    }


    public static string Decrypt(string message, string key)
    {
        string decryptedMsg = "";

        using (var aes = Aes.Create())
        {
            aes.Key = Encoding.ASCII.GetBytes(key);
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var buffer = Convert.FromBase64String(message);

            decryptedMsg = Encoding.ASCII.GetString(decryptor.TransformFinalBlock(buffer, 0, buffer.Length));
        }

        return decryptedMsg;
    }


    public static string hashSHA1(string message)
    {
        var hashedBytes = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(message));

        var hashedString = BitConverter.ToString(hashedBytes).Replace("-", "");
        return hashedString;
    }

}
