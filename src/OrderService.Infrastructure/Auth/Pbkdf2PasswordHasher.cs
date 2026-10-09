using System.Security.Cryptography;
using OrderService.Application.Abstractions;

namespace OrderService.Infrastructure.Auth;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Version = "v1";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 600_000;

    private static readonly byte[] DummySalt = new byte[SaltSize];

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Derive(password, salt, Iterations, HashSize);

        return $"{Version}.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string passwordHash, string password)
    {
        if (!TryParse(passwordHash, out var iterations, out var salt, out var expected))
        {
            Derive(password, DummySalt, Iterations, HashSize);
            return false;
        }

        var actual = Derive(password, salt, iterations, expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }

    private static byte[] Derive(string password, byte[] salt, int iterations, int length) =>
        Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, length);

    private static bool TryParse(string passwordHash, out int iterations, out byte[] salt, out byte[] hash)
    {
        iterations = 0;
        salt = [];
        hash = [];

        var parts = passwordHash.Split('.');

        if (parts.Length != 4 || parts[0] != Version || !int.TryParse(parts[1], out iterations) || iterations <= 0)
        {
            return false;
        }

        try
        {
            salt = Convert.FromBase64String(parts[2]);
            hash = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        return salt.Length > 0 && hash.Length > 0;
    }
}
