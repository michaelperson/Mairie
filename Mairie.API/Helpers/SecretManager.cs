using System.Security.Cryptography;
using System.Text;

namespace Mairie.API.Helpers
{
    public static class SecretManager
    {
        public static string Encrypt(string plainText)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
#pragma warning disable CA1416 // Validate platform compatibility
            byte[] encryptedBytes = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
#pragma warning restore CA1416 // Validate platform compatibility
            return Convert.ToBase64String(encryptedBytes);
        }
        public static string Decrypt(string encryptedText)
        {
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
#pragma warning disable CA1416 // Validate platform compatibility
            byte[] plainBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
#pragma warning restore CA1416 // Validate platform compatibility
            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
