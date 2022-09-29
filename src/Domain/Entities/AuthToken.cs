using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class AuthToken
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserId { get; set; }

    public string AccessTokenHash { get; set; }

    public DateTimeOffset AccessTokenExpiresDateTime { get; set; }

    public string RefreshTokenIdHash { get; set; }

    public string RefreshTokenIdHashSource { get; set; }

    public DateTimeOffset RefreshTokenExpiresDateTime { get; set; }

    public DateTime CreateDate { get; set; } = DateTime.Now;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; }
}
