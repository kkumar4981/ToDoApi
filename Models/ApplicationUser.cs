using Microsoft.AspNetCore.Identity;

namespace ToDoApi.Models;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public DateTime CreatedAtUtc { get; set; }
}
