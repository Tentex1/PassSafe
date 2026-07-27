namespace PassSafe.Services
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Implements military-grade AES-GCM (Galois/Counter Mode) encryption.
    /// This ensures authenticated encryption, protecting against both reading and tampering.
    /// </summary>
    public class AesCryptoService : ICryptoService
    {
        // Standard sizes for AES-GCM parameters
        private const int NonceSize = 12;
        private const int TagSize = 16;

        /// <summary>
        /// Encrypts the plain text using the Master Key.
        /// Combines the Nonce, Tag, and CipherText into a single Base64 string.
        /// </summary>
        public string Encrypt(string plainText, string masterKey)
        {
            if (string.IsNullOrEmpty(plainText)) return string.Empty;

            // Generate a 256-bit key by hashing the user's master password
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(masterKey));
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);

            // Generate a random 12-byte nonce (IV) for this specific encryption
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
            byte[] ciphertextBytes = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using (AesGcm aesGcm = new AesGcm(key, TagSize))
            {
                aesGcm.Encrypt(nonce, plaintextBytes, ciphertextBytes, tag);
            }

            // Combine Nonce + Tag + CipherText to store them safely together
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(nonce, 0, nonce.Length);
                ms.Write(tag, 0, tag.Length);
                ms.Write(ciphertextBytes, 0, ciphertextBytes.Length);

                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// Decrypts the payload by extracting the Nonce, Tag, and CipherText.
        /// Returns the plain text if the master key and authentication tag are valid.
        /// </summary>
        public string Decrypt(string cipherText, string masterKey)
        {
            if (string.IsNullOrEmpty(cipherText)) return string.Empty;

            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(masterKey));
            byte[] encryptedPayload = Convert.FromBase64String(cipherText);

            byte[] nonce = new byte[NonceSize];
            byte[] tag = new byte[TagSize];
            byte[] ciphertextBytes = new byte[encryptedPayload.Length - NonceSize - TagSize];

            // Extract the parts back from the payload array
            Buffer.BlockCopy(encryptedPayload, 0, nonce, 0, NonceSize);
            Buffer.BlockCopy(encryptedPayload, NonceSize, tag, 0, TagSize);
            Buffer.BlockCopy(encryptedPayload, NonceSize + TagSize, ciphertextBytes, 0, ciphertextBytes.Length);

            byte[] decryptedBytes = new byte[ciphertextBytes.Length];

            using (AesGcm aesGcm = new AesGcm(key, TagSize))
            {
                aesGcm.Decrypt(nonce, ciphertextBytes, tag, decryptedBytes);
            }

            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}