using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class User : IdentityUser<Guid>
{
    public User()
    {

    }

    public string SerialNumber { get; set; } = Guid.NewGuid().ToString("N");

    public virtual ICollection<AuthToken> AuthTokens { get; set; }
}