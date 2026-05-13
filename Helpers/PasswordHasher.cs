using System;
using System.Security.Cryptography;

namespace ValiModern.Helpers
{
    public static class PasswordHasher
    {
        private const string Prefix = "PBKDF2";
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int DefaultIterations = 100000;

        public static string HashPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, DefaultIterations, HashAlgorithmName.SHA256))
            {
                hash = pbkdf2.GetBytes(HashSize);
            }

            return string.Format(
                "{0}${1}${2}${3}",
                Prefix,
                DefaultIterations,
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash));
        }

        public static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword))
            {
                return false;
            }

            if (!IsHashedPassword(storedPassword))
            {
                return string.Equals(inputPassword ?? string.Empty, storedPassword);
            }

            try
            {
                var parts = storedPassword.Split('$');
                if (parts.Length != 4)
                {
                    return false;
                }

                if (!int.TryParse(parts[1], out var iterations))
                {
                    return false;
                }

                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);

                byte[] actualHash;
                using (var pbkdf2 = new Rfc2898DeriveBytes(inputPassword ?? string.Empty, salt, iterations, HashAlgorithmName.SHA256))
                {
                    actualHash = pbkdf2.GetBytes(expectedHash.Length);
                }

                return FixedTimeEquals(actualHash, expectedHash);
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }

        public static bool IsHashedPassword(string storedPassword)
        {
            return !string.IsNullOrEmpty(storedPassword) && storedPassword.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }
    }
}
