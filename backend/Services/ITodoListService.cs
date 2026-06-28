using TodoApi.DTOs.TodoLists;

namespace TodoApi.Services;

public interface ITodoListService
{
    Task<IEnumerable<TodoListResponse>> GetListsForUserAsync(Guid userId);
    Task<TodoListResponse> GetListAsync(Guid listId, Guid userId);
    Task<TodoListResponse> CreateListAsync(CreateTodoListRequest request, Guid userId);
    Task<TodoListResponse> UpdateListAsync(Guid listId, UpdateTodoListRequest request, Guid userId);
    Task DeleteListAsync(Guid listId, Guid userId);
}
