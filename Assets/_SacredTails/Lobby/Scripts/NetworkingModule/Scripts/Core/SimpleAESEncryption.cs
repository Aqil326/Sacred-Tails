using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class AESEncryption
{
    public const string Key = "ABCDEFGHJKLMNOPQRSTUVWXYZABCDEFG"; // must be 32 character
    public const string IV = "ABCDEFGHIJKLMNOP"; // must be 16 character
    public static string Encrypt(string message)
    {
        AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
        aes.BlockSize = 128;
        aes.KeySize = 256;
        aes.IV = UTF8Encoding.UTF8.GetBytes(IV);
        aes.Key = UTF8Encoding.UTF8.GetBytes(Key);
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        byte[] data = Encoding.UTF8.GetBytes(message);
        using (ICryptoTransform encrypt = aes.CreateEncryptor())
        {
            byte[] dest = encrypt.TransformFinalBlock(data, 0, data.Length);
            return Convert.ToBase64String(dest);
        }
    }

    public static string Decrypt(string encryptedText)
    {
        string plaintext = null;
        using (AesManaged aes = new AesManaged())
        {
            byte[] cipherText = Convert.FromBase64String(encryptedText);
            byte[] aesIV = UTF8Encoding.UTF8.GetBytes(IV);
            byte[] aesKey = UTF8Encoding.UTF8.GetBytes(Key);
            ICryptoTransform decryptor = aes.CreateDecryptor(aesKey, aesIV);
            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader reader = new StreamReader(cs))
                        plaintext = reader.ReadToEnd();
                }
            }
        }
        return plaintext;
    }
}

