namespace PassSafe.Services
{
    /// <summary>
    /// Provides encryption and decryption mechanisms to secure user data.
    /// </summary>
    public interface ICryptoService
    {
        string Encrypt(string plainText, string masterKey);
        string Decrypt(string cipherText, string masterKey);
    }
}