namespace TodoApi.DTOs.Todos;

public class TodoItemResponse
{
    public Guid Id { get; set; }
    public Guid TodoListId { get; set; }
    public required string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
