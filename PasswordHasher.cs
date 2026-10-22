using System.Security.Cryptography;
using System.Text;

namespace CFAS
{

    public static class PasswordHasher
    {
        /// <summary>
        /// Обчислює SHA-256 хеш пароля у вигляді
        /// 64 шістнадцяткових символів верхнього регістру.
        /// <summary>
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
