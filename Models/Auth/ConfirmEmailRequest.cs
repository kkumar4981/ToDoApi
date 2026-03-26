using System.ComponentModel.DataAnnotations;

namespace ToDoApi.Models.Auth;

public sealed class ConfirmEmailRequest
{
    [Required]
    public string UserId { get; init; } = string.Empty;

    [Required]
    public string Token { get; init; } = string.Empty;
}
