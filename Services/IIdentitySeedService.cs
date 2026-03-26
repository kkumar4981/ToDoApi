namespace ToDoApi.Services;

public interface IIdentitySeedService
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
