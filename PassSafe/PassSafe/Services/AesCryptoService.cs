namespace PassSafe.Services
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Defines the <see cref="AesCryptoService" />
    /// </summary>
    public class AesCryptoService : ICryptoService
    {
        private const int NonceSize = 12;// 96-bit nonce for AES-GCM

        private const int TagSize = 16;// 128-bit authentication tag

        public string Encrypt(string plainText, string masterKey)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            // Derive a secure 256-bit key from the masterKey string using SHA256
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(masterKey));
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);

            // Generate a unique random nonce (IV) for this encryption
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] ciphertextBytes = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            // Perform AES-GCM Encryption
            using (AesGcm aesGcm = new AesGcm(key, TagSize))
            {
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertextBytes, tag);
            }

            // Combine Nonce + Tag + Ciphertext into one single payload
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(nonce, 0, nonce.Length);
                ms.Write(tag, 0, tag.Length);
                ms.Write(ciphertextBytes, 0, ciphertextBytes.Length);

                return Convert.ToBase64String(ms.ToArray());
            }
        }

        public string Decrypt(string cipherText, string masterKey)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(masterKey));
            byte[] encryptedPayload = Convert.FromBase64String(cipherText);

            // Extract the sizes
            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] ciphertextBytes = new byte[encryptedPayload.Length - NonceSize - TagSize];

            // Parse the combined payload back into Nonce, Tag, and Ciphertext
            Buffer.BlockCopy(encryptedPayload, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(encryptedPayload, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(encryptedPayload, NonceSize + TagSize, ciphertextBytes, 0, ciphertextBytes.Length);

            byte[] decryptedBytes = new byte[ciphertextBytes.Length];

            // Perform AES-GCM Decryption
            using (AesGcm aesGcm = new AesGcm(key, TagSize))
            {
                aesGcm.Decrypt(nonce, ciphertextBytes, tag, decryptedBytes);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
