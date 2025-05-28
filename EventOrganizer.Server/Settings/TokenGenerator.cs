using System.Security.Cryptography;

public static class TokenGenerator
{
    public static string GenerateToken(int length = 32)
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(length));
    }
}