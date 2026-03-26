using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using ToDoApi.Models;
using ToDoApi.Models.Auth;
using ToDoApi.Repositories;

namespace ToDoApi.Services;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IRefreshTokenRepository refreshTokenRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IEmailService emailService,
    IHostEnvironment hostEnvironment) : IAuthService
{
    public async Task<ActionResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            LockoutEnabled = true
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        EnsureSucceeded(createResult);

        if (!await roleManager.RoleExistsAsync("User"))
        {
            var roleResult = await roleManager.CreateAsync(new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            });
            EnsureSucceeded(roleResult);
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, "User");
        EnsureSucceeded(addToRoleResult);

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await emailService.SendEmailAsync(
            user.Email!,
            "Confirm your email",
            $"UserId: {user.Id}\nToken: {token}",
            cancellationToken);

        return BuildActionResponse("Registration successful. Confirm the email before login.", user.Id, token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            throw new InvalidOperationException("Email is not confirmed.");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            throw new UnauthorizedAccessException("Account is locked.");
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            await userManager.AccessFailedAsync(user);

            if (await userManager.IsLockedOutAsync(user))
            {
                throw new UnauthorizedAccessException("Account is locked due to failed login attempts.");
            }

            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        return await CreateAuthResponseAsync(user, null, cancellationToken);
    }

    public async Task<ActionResponse> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            throw new InvalidOperationException("User was not found.");
        }

        var result = await userManager.ConfirmEmailAsync(user, request.Token);
        EnsureSucceeded(result);

        return new ActionResponse
        {
            Message = "Email confirmed successfully."
        };
    }

    public async Task<ActionResponse> ResendEmailConfirmationAsync(
        EmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return new ActionResponse
            {
                Message = "If the account exists, a confirmation email has been sent."
            };
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await emailService.SendEmailAsync(
            user.Email!,
            "Confirm your email",
            $"UserId: {user.Id}\nToken: {token}",
            cancellationToken);

        return BuildActionResponse("Confirmation email generated.", user.Id, token);
    }

    public async Task<ActionResponse> ForgotPasswordAsync(
        EmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await userManager.IsEmailConfirmedAsync(user))
        {
            return new ActionResponse
            {
                Message = "If the account exists, a reset email has been sent."
            };
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await emailService.SendEmailAsync(
            user.Email!,
            "Reset your password",
            $"Email: {user.Email}\nToken: {token}",
            cancellationToken);

        return BuildActionResponse("Password reset token generated.", user.Id, token);
    }

    public async Task<ActionResponse> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            throw new InvalidOperationException("User was not found.");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        EnsureSucceeded(result);

        await userManager.UpdateSecurityStampAsync(user);
        await refreshTokenRepository.RevokeAllForUserAsync(user.Id, DateTime.UtcNow, null, cancellationToken);

        return new ActionResponse
        {
            Message = "Password reset successfully."
        };
    }

    public async Task<AuthResponse> RefreshAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken is null || existingToken.RevokedAtUtc.HasValue || existingToken.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Refresh token is invalid.");
        }

        var user = await userManager.FindByIdAsync(existingToken.UserId.ToString());
        if (user is null)
        {
            throw new UnauthorizedAccessException("Refresh token is invalid.");
        }

        if (!string.Equals(user.SecurityStamp, existingToken.SecurityStamp, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Refresh token is invalid.");
        }

        await refreshTokenRepository.RevokeAsync(existingToken.Id, DateTime.UtcNow, ipAddress, cancellationToken);
        return await CreateAuthResponseAsync(user, ipAddress, cancellationToken);
    }

    public async Task<ActionResponse> RevokeRefreshTokenAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default)
    {
        var existingToken = await refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);
        if (existingToken is null || existingToken.RevokedAtUtc.HasValue)
        {
            return new ActionResponse
            {
                Message = "Refresh token already inactive."
            };
        }

        await refreshTokenRepository.RevokeAsync(existingToken.Id, DateTime.UtcNow, ipAddress, cancellationToken);

        return new ActionResponse
        {
            Message = "Refresh token revoked."
        };
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(
        ApplicationUser user,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = jwtTokenGenerator.GenerateToken(user, roles);

        var refreshToken = new ApplicationRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = GenerateRefreshTokenValue(),
            SecurityStamp = user.SecurityStamp ?? string.Empty,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress
        };

        await refreshTokenRepository.CreateAsync(refreshToken, cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken.Token,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = refreshToken.Token,
            RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Roles = roles.ToArray()
        };
    }

    private ActionResponse BuildActionResponse(string message, Guid userId, string token)
    {
        return new ActionResponse
        {
            Message = message,
            UserId = userId.ToString(),
            Token = hostEnvironment.IsDevelopment() ? token : null
        };
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
    }

    private static string GenerateRefreshTokenValue()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
}
