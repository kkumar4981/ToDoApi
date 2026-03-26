using ToDoApi.Models.Auth;

namespace ToDoApi.Services;

public interface IAuthService
{
    Task<ActionResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<ActionResponse> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default);

    Task<ActionResponse> ResendEmailConfirmationAsync(EmailRequest request, CancellationToken cancellationToken = default);

    Task<ActionResponse> ForgotPasswordAsync(EmailRequest request, CancellationToken cancellationToken = default);

    Task<ActionResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> RefreshAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);

    Task<ActionResponse> RevokeRefreshTokenAsync(
        RefreshTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken = default);
}
