namespace TodoApi.Services;

public interface INotificationService
{
    public Task SendReminderAsync(string userEmail, string todoTitle, DateTime dueDate);
}
