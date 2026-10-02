namespace VGameFoundation.Scripts.Utilities.Data
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// AES-CBC over UTF8 text, with a freshly generated IV prefixed to the cipher text and the
    /// whole thing base64 encoded. The payload format must never change or previously saved
    /// data becomes unreadable.
    /// </summary>
    public sealed class AesTextCipher
    {
        private const int KeySizeBytes = 32;
        private const int IvSizeBytes  = 16;

        private readonly byte[] key;

        public AesTextCipher(string passphrase)
        {
            var passphraseBytes = Encoding.UTF8.GetBytes(passphrase);

            this.key = new byte[KeySizeBytes];
            Array.Copy(passphraseBytes, this.key, Math.Min(passphraseBytes.Length, KeySizeBytes));
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = this.key;
            aes.GenerateIV();

            using var stream = new MemoryStream();
            stream.Write(aes.IV, 0, aes.IV.Length);

            using (var cryptoStream = new CryptoStream(stream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cryptoStream))
            {
                writer.Write(plainText);
            }

            return Convert.ToBase64String(stream.ToArray());
        }

        /// <summary>
        /// Never throws: unencrypted leftovers, a truncated write or a changed passphrase all
        /// simply report failure so callers can fall back to their own defaults.
        /// </summary>
        public bool TryDecrypt(string cipherText, out string plainText)
        {
            plainText = null;

            if (string.IsNullOrEmpty(cipherText)) return false;

            try
            {
                var payload = Convert.FromBase64String(cipherText);
                if (payload.Length <= IvSizeBytes) return false;

                var iv = new byte[IvSizeBytes];
                Array.Copy(payload, iv, IvSizeBytes);

                using var aes = Aes.Create();
                aes.Key = this.key;

                using var stream       = new MemoryStream(payload, IvSizeBytes, payload.Length - IvSizeBytes);
                using var cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(aes.Key, iv), CryptoStreamMode.Read);
                using var reader       = new StreamReader(cryptoStream);

                plainText = reader.ReadToEnd();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
