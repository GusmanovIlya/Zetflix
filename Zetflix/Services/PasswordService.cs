using System.Security.Cryptography;

namespace Zetflix.Services;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    bool IsHashed(string passwordHash);
}

public class PasswordService : IPasswordService
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize
        );

        return $"pbkdf2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (!IsHashed(passwordHash))
        {
            return password == passwordHash;
        }

        var parts = passwordHash.Split('$');

        if (parts.Length != 4 || !int.TryParse(parts[1], out var iterations))
        {
            return false;
        }

        var salt = Convert.FromBase64String(parts[2]);
        var expectedKey = Convert.FromBase64String(parts[3]);
        var actualKey = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expectedKey.Length
        );

        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey);
    }

    public bool IsHashed(string passwordHash)
    {
        return passwordHash.StartsWith("pbkdf2$", StringComparison.Ordinal);
    }
}
