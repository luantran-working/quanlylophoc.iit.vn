using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ClassroomManagement.Services
{
    /// <summary>
    /// Service for IP-based connection password
    /// </summary>
    public class ConnectionPasswordService
    {
        private static ConnectionPasswordService? _instance;
        public static ConnectionPasswordService Instance => _instance ??= new ConnectionPasswordService();

        private ConnectionPasswordService() { }

        /// <summary>
        /// Generate a short password hash from IP address (5-7 characters)
        /// </summary>
        public string GeneratePasswordFromIP(string ipAddress)
        {
            try
            {
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(ipAddress));

                // Convert to base36 for shorter, more readable password
                var base36Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                var password = new StringBuilder();

                // Use first 5-6 bytes of hash
                for (int i = 0; i < 6; i++)
                {
                    var value = hash[i] % 36;
                    password.Append(base36Chars[value]);
                }

                return password.ToString();
            }
            catch
            {
                // Fallback to random if IP is invalid
                return GenerateRandomPassword();
            }
        }

        /// <summary>
        /// Generate a random password (fallback)
        /// </summary>
        private string GenerateRandomPassword()
        {
            var chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var random = new Random();
            return new string(Enumerable.Range(0, 6)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }

        /// <summary>
        /// Validate if password matches IP
        /// </summary>
        public bool ValidatePassword(string ipAddress, string password)
        {
            var expectedPassword = GeneratePasswordFromIP(ipAddress);
            return string.Equals(expectedPassword, password, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get IP from password by trying common IP ranges (optional helper)
        /// </summary>
        public string? TryGetIPFromPassword(string password)
        {
            // This is a reverse lookup - try common local IP ranges
            for (int a = 192; a <= 192; a++)
            {
                for (int b = 168; b <= 168; b++)
                {
                    for (int c = 0; c <= 255; c++)
                    {
                        for (int d = 1; d <= 254; d++)
                        {
                            var testIp = $"{a}.{b}.{c}.{d}";
                            if (ValidatePassword(testIp, password))
                            {
                                return testIp;
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}
