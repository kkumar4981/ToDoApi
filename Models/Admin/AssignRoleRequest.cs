using System.ComponentModel.DataAnnotations;

namespace ToDoApi.Models.Admin;

public sealed class AssignRoleRequest
{
    [Required]
    public string RoleName { get; init; } = string.Empty;
}
