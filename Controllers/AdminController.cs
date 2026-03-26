using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ToDoApi.Models.Admin;
using ToDoApi.Models.Auth;
using ToDoApi.Services;

namespace ToDoApi.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<UserAdminResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await adminService.GetUsersAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("roles")]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await adminService.GetRolesAsync(cancellationToken);
        return Ok(roles);
    }

    [HttpPost("roles")]
    public async Task<ActionResult<ActionResponse>> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await adminService.CreateRoleAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("users/{userId}/roles")]
    public async Task<ActionResult<ActionResponse>> AssignRole(
        string userId,
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await adminService.AssignRoleAsync(userId, request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("users/{userId}/roles/{roleName}")]
    public async Task<ActionResult<ActionResponse>> RemoveRole(
        string userId,
        string roleName,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await adminService.RemoveRoleAsync(userId, roleName, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("users/{userId}/unlock")]
    public async Task<ActionResult<ActionResponse>> UnlockUser(
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await adminService.UnlockUserAsync(userId, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }
}
