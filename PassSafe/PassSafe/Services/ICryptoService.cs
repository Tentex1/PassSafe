namespace PassSafe.Services
{
    /// <summary>
    /// Defines the <see cref="ICryptoService" />
    /// </summary>
    public interface ICryptoService
    {
        string Encrypt(string plainText, string masterKey);

        string Decrypt(string cipherText, string masterKey);
    }
}
