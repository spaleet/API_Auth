using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Context;

public class DatabaseContext : IdentityDbContext<User, UserRole, Guid>, IApplicationDbContext
{
    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<AuthToken> AuthTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}