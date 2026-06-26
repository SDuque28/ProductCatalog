using System.Security.Cryptography;
using System.Text;
using ProductCatalog.Api.Interfaces;

namespace ProductCatalog.Api.Services;

public class Sha256PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }

    public bool Verify(string password, string passwordHash)
    {
        var computedHash = Hash(password);

        return string.Equals(computedHash, passwordHash, StringComparison.Ordinal);
    }
}
