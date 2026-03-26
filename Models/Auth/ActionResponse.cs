namespace ToDoApi.Models.Auth;

public sealed class ActionResponse
{
    public string Message { get; init; } = string.Empty;

    public string? UserId { get; init; }

    public string? Token { get; init; }
}
