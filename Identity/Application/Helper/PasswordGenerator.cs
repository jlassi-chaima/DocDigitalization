

using System.Security.Cryptography;

namespace Application.Helper
{
    public static class PasswordGenerator
    {
        private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string Digits = "0123456789";
        private const string SpecialChars = "!@#$%^&*()-_=+<>?";
        private const int PasswordLength = 12; // Set the desired password length

        public static string GenerateSecurePassword(int length = PasswordLength)
        {
            if (length < 6)
                throw new ArgumentException("Password length should be at least 6 characters");

            string allChars = UpperCase + LowerCase + Digits + SpecialChars;
            char[] password = new char[length];
            byte[] randomBytes = new byte[length];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(randomBytes);
            }

            for (int i = 0; i < length; i++)
            {
                password[i] = allChars[randomBytes[i] % allChars.Length];
            }

            // Ensure password contains at least one character from each category
            password[0] = UpperCase[randomBytes[0] % UpperCase.Length];
            password[1] = LowerCase[randomBytes[1] % LowerCase.Length];
            password[2] = Digits[randomBytes[2] % Digits.Length];
            password[3] = SpecialChars[randomBytes[3] % SpecialChars.Length];

            // Shuffle the characters for extra security
            return new string(password.OrderBy(c => randomBytes[new Random().Next(randomBytes.Length)]).ToArray());
        }
    }
}
