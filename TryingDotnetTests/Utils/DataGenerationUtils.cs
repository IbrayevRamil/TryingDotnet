using System.Security.Cryptography;

namespace TryingDotnetTests.Utils;

public static class DataGenerationUtils
{
    public static string GenerateRandomString(int length = 12)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var stringChars = new char[length];
        var randomBytes = new byte[length];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        for (var i = 0; i < length; i++)
        {
            var randomIndex = randomBytes[i] % chars.Length;
            stringChars[i] = chars[randomIndex];
        }

        return new string(stringChars);
    }
}