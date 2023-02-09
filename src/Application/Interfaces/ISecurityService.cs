namespace Application.Interfaces;

public interface ISecurityService
{
    string GetSha256Hash(string input);
    Guid CreateCryptographicallySecureGuid();
}