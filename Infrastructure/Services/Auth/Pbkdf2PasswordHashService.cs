using System.Security.Cryptography;
using Application.Abstractions.Auth;

namespace Infrastructure.Services.Auth;

public sealed class Pbkdf2PasswordHashService : IPasswordHashService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 120_000;
    private const string Algorithm = "SHA256";

    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

        var payload = $"{Iterations}.{Algorithm}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        return payload;
    }

    public bool Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var segments = passwordHash.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length != 4 || !int.TryParse(segments[0], out var iterations))
        {
            return false;
        }

        if (!string.Equals(segments[1], Algorithm, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        byte[] salt;
        byte[] expectedHash;
        try
        {
            salt = Convert.FromBase64String(segments[2]);
            expectedHash = Convert.FromBase64String(segments[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedHash.Length);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
