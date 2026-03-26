using System.ComponentModel.DataAnnotations;

namespace ToDoApi.Models.Auth;

public sealed class RegisterRequest
{
    [Required]
    [MinLength(3)]
    public string UserName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; init; } = string.Empty;
}
