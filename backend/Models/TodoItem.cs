namespace TodoApi.Models;

public class TodoItem
{
    public Guid Id { get; set; }
    public Guid TodoListId { get; set; }
    public TodoList TodoList { get; set; } = null!;
    public required string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ReminderSentAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
