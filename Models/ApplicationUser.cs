using Microsoft.AspNetCore.Identity;

namespace Kadikoy.Models;

public class ApplicationUser : IdentityUser<int>
{
    public string? FullName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

