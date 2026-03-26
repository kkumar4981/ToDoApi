using System.ComponentModel.DataAnnotations;

namespace ToDoApi.Models.Auth;

public sealed class EmailRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}
