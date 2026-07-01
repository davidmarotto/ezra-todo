using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;

namespace TodoApi.Services;

public class ReminderSettings
{
    public int PollingIntervalMinutes { get; set; }
    public int LeadTimeHours { get; set; }
}

public class ReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ReminderBackgroundService> _logger;
    private readonly ReminderSettings _settings;

    public ReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        INotificationService notificationService,
        IOptions<ReminderSettings> settings,
        ILogger<ReminderBackgroundService> logger
        )
    {
        _scopeFactory = scopeFactory;
        _notificationService = notificationService;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reminder service started...");

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckRemindersAsync();
            await Task.Delay(TimeSpan.FromMinutes(_settings.PollingIntervalMinutes), stoppingToken);
        }
    }

    private async Task CheckRemindersAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var now = DateTime.UtcNow;
        var cutoff = now.AddHours(_settings.LeadTimeHours);

        var dueTodos = await db.TodoItems
            .Include(t => t.TodoList)
            .ThenInclude(l => l.Owner)
            .Where(t =>
                !t.IsCompleted &&
                t.DueDate.HasValue &&
                t.DueDate <= cutoff &&
                t.ReminderSentAt == null)
            .ToListAsync();

        foreach (var todo in dueTodos)
        {
            await _notificationService.SendReminderAsync(
                todo.TodoList.Owner.Email,
                todo.Title,
                todo.DueDate!.Value);
            
            todo.ReminderSentAt = DateTime.UtcNow;
        }

        if (dueTodos.Count > 0)
            await db.SaveChangesAsync();

    }

}
