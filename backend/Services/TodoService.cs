using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs.Todos;
using TodoApi.Models;

namespace TodoApi.Services;

public class TodoService : ITodoService
{
    private readonly AppDbContext _context;

    public TodoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TodoItemResponse>> GetTodosAsync(Guid listId, Guid userId, string? status)
    {
        if (!await CanAccessListAsync(listId, userId))
            throw new KeyNotFoundException("List not found.");

        var query = _context.TodoItems.Where(t => t.TodoListId == listId);

        query = status switch
        {
            "active" => query.Where(t => !t.IsCompleted),
            "completed" => query.Where(t => t.IsCompleted),
            _ => query
        };

        return await query.Select(t => ToResponse(t)).ToListAsync();
    }

    public async Task<TodoItemResponse> GetTodoAsync(Guid listId, Guid todoId, Guid userId)
    {
        if (!await CanAccessListAsync(listId, userId))
            throw new KeyNotFoundException("List not found.");

        var todo = await _context.TodoItems
            .SingleOrDefaultAsync(t => t.Id == todoId && t.TodoListId == listId);

        if (todo == null)
            throw new KeyNotFoundException("Todo not found.");

        return ToResponse(todo);
    }

    public async Task<TodoItemResponse> CreateTodoAsync(Guid listId, CreateTodoRequest request, Guid userId)
    {
        if (!await CanEditListAsync(listId, userId))
            throw new UnauthorizedAccessException("You do not have permission to add todos to this list.");

        var todo = new TodoItem
        {
            TodoListId = listId,
            Title = request.Title,
            DueDate = request.DueDate
        };

        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync();

        return ToResponse(todo);
    }

    public async Task<TodoItemResponse> UpdateTodoAsync(Guid listId, Guid todoId, UpdateTodoRequest request, Guid userId)
    {
        if (!await CanEditListAsync(listId, userId))
            throw new UnauthorizedAccessException("You do not have permission to update todos in this list.");

        var todo = await _context.TodoItems
            .SingleOrDefaultAsync(t => t.Id == todoId && t.TodoListId == listId);

        if (todo == null)
            throw new KeyNotFoundException("Todo not found.");

        todo.Title = request.Title;
        todo.IsCompleted = request.IsCompleted;
        todo.DueDate = request.DueDate;
        todo.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return ToResponse(todo);
    }

    public async Task DeleteTodoAsync(Guid listId, Guid todoId, Guid userId)
    {
        if (!await CanEditListAsync(listId, userId))
            throw new UnauthorizedAccessException("You do not have permission to delete todos from this list.");

        var todo = await _context.TodoItems
            .SingleOrDefaultAsync(t => t.Id == todoId && t.TodoListId == listId);

        if (todo == null)
            throw new KeyNotFoundException("Todo not found.");

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync();
    }

    private async Task<bool> CanAccessListAsync(Guid listId, Guid userId)
    {
        var list = await _context.TodoLists.FindAsync(listId);
        if (list == null) return false;
        if (list.OwnerId == userId) return true;

        return await _context.ListPermissions
            .AnyAsync(lp => lp.TodoListId == listId && lp.UserId == userId);
    }

    private async Task<bool> CanEditListAsync(Guid listId, Guid userId)
    {
        var list = await _context.TodoLists.FindAsync(listId);
        if (list == null) return false;
        if (list.OwnerId == userId) return true;

        return await _context.ListPermissions
            .AnyAsync(lp => lp.TodoListId == listId && lp.UserId == userId && lp.Role == PermissionRole.Editor);
    }

    private static TodoItemResponse ToResponse(TodoItem todo) => new()
    {
        Id = todo.Id,
        TodoListId = todo.TodoListId,
        Title = todo.Title,
        IsCompleted = todo.IsCompleted,
        DueDate = todo.DueDate,
        CreatedAt = todo.CreatedAt,
        UpdatedAt = todo.UpdatedAt
    };
}
