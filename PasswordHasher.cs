using System.Security.Cryptography;
using System.Text;

namespace CFAS
{
    /// <summary>
    /// Хешування паролів за стандартом SHA-256.
    /// </summary>
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder(64);
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("X2"));
                }
                return builder.ToString();
            }
        }
    }
}