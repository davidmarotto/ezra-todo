namespace TodoApi.Services;

class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendReminderAsync(string userEmail, string todoTitle, DateTime dueDate)
    {
        _logger.LogInformation(
            "REMINDER: {Email} — '{Title}' is due {DueDate:yyyy-MM-dd}",
            userEmail, todoTitle, dueDate
        );

        return Task.CompletedTask;
    }
}