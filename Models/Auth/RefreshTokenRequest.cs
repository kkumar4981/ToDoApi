using System.ComponentModel.DataAnnotations;

namespace ToDoApi.Models.Auth;

public sealed class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}
