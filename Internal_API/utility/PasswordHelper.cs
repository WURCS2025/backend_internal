using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System;

namespace Internal_API.utility
{
    public static class PasswordHelper
    {
        public static string HashPassword(this string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        public static bool VerifyPassword(this string enteredPassword, string storedHash)
        {
            return HashPassword(enteredPassword) == storedHash;
        }

        public static bool IsValidPassword(this string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            // Regular expression for password validation
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$";

            return Regex.IsMatch(password, pattern);
        }
    }
}
