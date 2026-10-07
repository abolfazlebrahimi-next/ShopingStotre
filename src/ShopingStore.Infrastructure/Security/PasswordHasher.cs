using System.Security.Cryptography;

namespace ShopingStore.Infrastructure.Security;

/// <summary>
/// هش و بررسی رمز عبور با الگوریتم PBKDF2-SHA256 (۱۰۰٬۰۰۰ تکرار) و نمک تصادفی.
/// قالب ذخیره‌سازی: PBKDF2$iterations$saltBase64$hashBase64
/// </summary>
public static class PasswordHasher
{
    private const string Prefix = "PBKDF2";
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;

    public static string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return $"{Prefix}${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(key)}";
    }

    public static bool Verify(string? password, string? hash, out bool needsRehash)
    {
        needsRehash = false;

        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash)) return false;

        var parts = hash.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix) return false;

        if (!int.TryParse(parts[1], out var iterations)) return false;

        byte[] salt, expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        var matches = CryptographicOperations.FixedTimeEquals(actual, expected);

        if (matches && iterations != Iterations) needsRehash = true;

        return matches;
    }

    public static bool Verify(string? password, string? hash) => Verify(password, hash, out _);
}
