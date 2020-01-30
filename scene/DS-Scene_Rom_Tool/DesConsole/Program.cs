namespace DesConsole
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Threading.Tasks;

    class Program
    {
        static async Task Main(string[] args)
        {
            await DecryptAsync(args[0], args[2], args[1]);
        }

        static async Task DecryptAsync(string fileIn, string key, string fileOut)
        {
            var binaryKey = new byte[8];
            for (int i = 0; i < binaryKey.Length; i++) {
                binaryKey[i] = byte.Parse(key.Substring(i * 2, 2), NumberStyles.HexNumber);
            }

            using var des = DES.Create();
            using var decryptor = des.CreateDecryptor(binaryKey, binaryKey);

            using var streamIn = new FileStream(fileIn, FileMode.Open);
            using var streamOut = new FileStream(fileOut, FileMode.Create);

            using var decryptedStream = new CryptoStream(streamIn, decryptor, CryptoStreamMode.Read);
            await decryptedStream.CopyToAsync(streamOut);
        }
    }
}
