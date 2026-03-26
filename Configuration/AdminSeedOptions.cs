namespace ToDoApi.Configuration;

public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public bool Enabled { get; set; } = true;

    public string UserName { get; set; } = "admin";

    public string Email { get; set; } = "admin@todoapi.local";

    public string Password { get; set; } = "Admin123!";
}
