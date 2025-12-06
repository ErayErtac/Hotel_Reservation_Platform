using BCrypt.Net;

namespace HotelReservation.Core.Services
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            // BCrypt hash'leri genellikle "$2a$", "$2b$", "$2y$" ile başlar
            // Eğer hash BCrypt formatında değilse, eski plain text şifre olabilir
            if (string.IsNullOrEmpty(hashedPassword))
                return false;

            bool isBcryptHash = hashedPassword.StartsWith("$2") && hashedPassword.Length > 20;

            if (isBcryptHash)
            {
                try
                {
                    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }
                catch
                {
                    // BCrypt parse hatası - eski format olabilir
                    return false;
                }
            }
            else
            {
                // Eski plain text şifre - direkt karşılaştır
                return password == hashedPassword;
            }
        }

        /// <summary>
        /// Şifrenin BCrypt formatında olup olmadığını kontrol eder
        /// </summary>
        public static bool IsBcryptHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            return hash.StartsWith("$2") && hash.Length > 20;
        }
    }
}

