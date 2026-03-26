using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ToDoApi.Configuration;
using ToDoApi.Models;

namespace ToDoApi.Services;

public sealed class IdentitySeedService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<AdminSeedOptions> adminSeedOptions) : IIdentitySeedService
{
    private readonly AdminSeedOptions _options = adminSeedOptions.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureRoleAsync("Admin");
        await EnsureRoleAsync("User");

        if (!_options.Enabled)
        {
            return;
        }

        var existingUser = await userManager.FindByEmailAsync(_options.Email);
        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, "Admin"))
            {
                var existingResult = await userManager.AddToRoleAsync(existingUser, "Admin");
                EnsureSucceeded(existingResult);
            }

            return;
        }

        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = _options.UserName,
            Email = _options.Email,
            EmailConfirmed = true,
            CreatedAtUtc = DateTime.UtcNow,
            LockoutEnabled = true,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };

        var createResult = await userManager.CreateAsync(adminUser, _options.Password);
        EnsureSucceeded(createResult);

        var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");
        EnsureSucceeded(roleResult);
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await roleManager.CreateAsync(new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant(),
            ConcurrencyStamp = Guid.NewGuid().ToString()
        });

        EnsureSucceeded(result);
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
    }
}
