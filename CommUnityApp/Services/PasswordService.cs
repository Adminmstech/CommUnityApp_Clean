using System.Security.Cryptography;

namespace CommUnityApp.Services
{
    public interface IPasswordService
    {
        string GenerateSecurePassword(int length = 8);
    }

    public class PasswordService : IPasswordService
    {
        private const string PasswordChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public string GenerateSecurePassword(int length = 8)
        {
            if (length < 4 || length > 128)
                throw new ArgumentException("Password length must be between 4 and 128 characters.", nameof(length));

            Span<char> result = stackalloc char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = PasswordChars[RandomNumberGenerator.GetInt32(PasswordChars.Length)];
            }

            return new string(result);
        }
    }
}
