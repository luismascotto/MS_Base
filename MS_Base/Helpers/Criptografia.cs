using System;
using System.Security.Cryptography;
using System.Text;

namespace MS_Base.Helpers
{
    public static class Criptografia
    {
        public static string strKey = "0t0rr1n0l@r1ng0l0g1@1234";
        public static string strDBKey = "M@r1@B3t@n1a1234";

        public static string Encrypt(string message, string key)
        {
            string encryptedMsg = "";

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.ASCII.GetBytes(key);
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                {
                    var buffer = Encoding.ASCII.GetBytes(message);

                    encryptedMsg = Convert.ToBase64String(encryptor.TransformFinalBlock(buffer, 0, buffer.Length));
                }
            }

            return encryptedMsg;
        }

        public static string Encrypt(string message)
        {
            return Encrypt(message, strKey);
        }

        public static string Decrypt(string message, string key)
        {
            string decryptedMsg = "";

            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.ASCII.GetBytes(key);
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor())
                {
                    var buffer = Convert.FromBase64String(message);

                    decryptedMsg = Encoding.ASCII.GetString(decryptor.TransformFinalBlock(buffer, 0, buffer.Length));
                }
            }

            return decryptedMsg;
        }

        public  static string Decrypt(string message)
        {
            return Decrypt(message, strKey);
        }

        public static string hashSHA1(string message)
        {
            var hashedBytes = SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(message));

            var hashedString = BitConverter.ToString(hashedBytes).Replace("-", "");
            return hashedString;
        }

    }
}
