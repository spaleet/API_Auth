using System.Security.Cryptography;
using System.Text;

namespace Application.Services;

public class SecurityService : ISecurityService
{
    private readonly RandomNumberGenerator _rand = RandomNumberGenerator.Create();

    public string GetSha256Hash(string input)
    {
        using var hashAlgorithm = SHA256.Create();
        byte[] byteValue = Encoding.UTF8.GetBytes(input);
        byte[] byteHash = hashAlgorithm.ComputeHash(byteValue);
        return Convert.ToBase64String(byteHash);
    }

    public Guid CreateCryptographicallySecureGuid()
    {
        byte[] bytes = new byte[16];
        _rand.GetBytes(bytes);
        return new Guid(bytes);
    }
}
