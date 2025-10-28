using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Kadikoy.Models;

namespace Kadikoy.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Seed roles
        builder.Entity<ApplicationRole>().HasData(
            new ApplicationRole("Admin") { Id = 1, NormalizedName = "ADMIN" },
            new ApplicationRole("PhotoUploader") { Id = 2, NormalizedName = "PHOTOUPLOADER" },
            new ApplicationRole("User") { Id = 3, NormalizedName = "USER" }
        );

        // Seed admin user
        var hasher = new PasswordHasher<ApplicationUser>();
        var adminUser = new ApplicationUser
        {
            Id = 1,
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@kadikoy.com",
            NormalizedEmail = "ADMIN@KADIKOY.COM",
            EmailConfirmed = true,
            FullName = "System Administrator",
            SecurityStamp = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.UtcNow
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin123!");

        builder.Entity<ApplicationUser>().HasData(adminUser);

        // Assign admin role to admin user
        builder.Entity<IdentityUserRole<int>>().HasData(
            new IdentityUserRole<int>
            {
                RoleId = 1,
                UserId = 1
            }
        );
    }
}

