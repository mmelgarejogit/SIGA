using System.Security.Cryptography;
using SIGA.Domain.Security;

namespace SIGA.Infrastructure.Security;

public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
    
    public string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return $"pbkdf2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        if (string.IsNullOrWhiteSpace(storedHash)) return false;

        var parts = storedHash.Split('$');
        if (parts.Length != 4) return false;
        if (!parts[0].Equals("pbkdf2", StringComparison.Ordinal)) return false;

        if (!int.TryParse(parts[1], out var iterations)) return false;

        byte[] salt;
        byte[] key;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            key = Convert.FromBase64String(parts[3]);
        }
        catch
        {
            return false;
        }
        
        var computedKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, key.Length);
        
        return CryptographicOperations.FixedTimeEquals(computedKey, key);
    }
}