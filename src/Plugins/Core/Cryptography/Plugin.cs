using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;

namespace SKHelpers.Plugins.Cryptography;
public class AESCryptographyPlugin(string key, string iv)
{
    [KernelFunction]
    [Description("Encrypts a string using AES.")]
    [return: Description("The encrypted string in Base64 format.")]
    public string EncryptAesAsync(
        [Description("The input string to encrypt.")]
        string input)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(key);
        aes.IV = Convert.FromBase64String(iv);

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream memoryStream = new();
        using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
        using StreamWriter streamWriter = new(cryptoStream);
        streamWriter.Write(input);

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    [KernelFunction]
    [Description("Decrypts a string using AES.")]
    [return: Description("The decrypted string.")]
    public string DecryptAes(
        [Description("The input string to decrypt.")]
        string input)
    {
        using var aes = Aes.Create();
        aes.Key = Convert.FromBase64String(key);
        aes.IV = Convert.FromBase64String(iv);

        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream memoryStream = new(Convert.FromBase64String(input));
        using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new(cryptoStream);
        return streamReader.ReadToEnd();
    }
}

public class CryptographyPlugin
{
    [KernelFunction]
    [Description("Hashes a string using SHA256.")]
    [return: Description("The hashed string in hexadecimal format.")]
    public string HashSha256(
        [Description("The input string to hash.")]
        string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    [KernelFunction]
    [Description("Generates a random encryption key and IV for AES.")]
    [return: Description("The generated key and IV in Base64 format.")]
    public (string key, string iv) GenerateAesKeyAndIv()
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        var key = Convert.ToBase64String(aes.Key);
        var iv = Convert.ToBase64String(aes.IV);
        return (key, iv);
    }
}
