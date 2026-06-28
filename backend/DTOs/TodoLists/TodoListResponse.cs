namespace TodoApi.DTOs.TodoLists;

public class TodoListResponse
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
