namespace ToDoApi.Services;

public sealed class DevelopmentEmailService(ILogger<DevelopmentEmailService> logger) : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Development email.\nTo: {To}\nSubject: {Subject}\nBody:\n{Body}",
            to,
            subject,
            body);

        return Task.CompletedTask;
    }
}
