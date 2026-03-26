using System.ComponentModel.DataAnnotations;

namespace ToDoApi.Models.Admin;

public sealed class CreateRoleRequest
{
    [Required]
    public string RoleName { get; init; } = string.Empty;
}
