using TodoApi.DTOs.Todos;

namespace TodoApi.Services;

public interface ITodoService
{
    Task<IEnumerable<TodoItemResponse>> GetTodosAsync(Guid listId, Guid userId, string? status);
    Task<TodoItemResponse> GetTodoAsync(Guid listId, Guid todoId, Guid userId);
    Task<TodoItemResponse> CreateTodoAsync(Guid listId, CreateTodoRequest request, Guid userId);
    Task<TodoItemResponse> UpdateTodoAsync(Guid listId, Guid todoId, UpdateTodoRequest request, Guid userId);
    Task DeleteTodoAsync(Guid listId, Guid todoId, Guid userId);
}
