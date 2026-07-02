using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs.TodoLists;
using TodoApi.Models;

namespace TodoApi.Services;

public class TodoListService : ITodoListService
{
    private readonly AppDbContext _context;

    public TodoListService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TodoListResponse>> GetListsForUserAsync(Guid userId)
    {
        var owned = await _context.TodoLists
            .Where(l => l.OwnerId == userId)
            .Select(l => new TodoListResponse
            {
                Id = l.Id,
                Name = l.Name,
                Role = "Owner",
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt
            })
            .ToListAsync();

        var sharedPermissions = await _context.ListPermissions
            .Where(lp => lp.UserId == userId)
            .Include(lp => lp.TodoList)
            .ToListAsync();

        var shared = sharedPermissions.Select(lp => new TodoListResponse
        {
            Id = lp.TodoList.Id,
            Name = lp.TodoList.Name,
            Role = lp.Role.ToString(),
            CreatedAt = lp.TodoList.CreatedAt,
            UpdatedAt = lp.TodoList.UpdatedAt
        });

        return owned.Concat(shared);
    }

    public async Task<TodoListResponse> GetListAsync(Guid listId, Guid userId)
    {
        var role = await GetUserRoleAsync(listId, userId);
        if (role == null)
            throw new KeyNotFoundException("List not found.");

        var list = await _context.TodoLists.FindAsync(listId);
        return ToResponse(list!, role);
    }

    public async Task<TodoListResponse> CreateListAsync(CreateTodoListRequest request, Guid userId)
    {
        var list = new TodoList
        {
            Name = request.Name,
            OwnerId = userId
        };

        _context.TodoLists.Add(list);
        await _context.SaveChangesAsync();

        return ToResponse(list, "Owner");
    }

    public async Task<TodoListResponse> UpdateListAsync(Guid listId, UpdateTodoListRequest request, Guid userId)
    {
        var role = await GetUserRoleAsync(listId, userId);
        if (role == null)
            throw new KeyNotFoundException("List not found.");
        if (role != "Owner")
            throw new UnauthorizedAccessException("Only the owner can update this list.");

        var list = await _context.TodoLists.FindAsync(listId);
        list!.Name = request.Name;
        list.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ToResponse(list, role);
    }

    public async Task DeleteListAsync(Guid listId, Guid userId)
    {
        var role = await GetUserRoleAsync(listId, userId);
        if (role == null)
            throw new KeyNotFoundException("List not found.");
        if (role != "Owner")
            throw new UnauthorizedAccessException("Only the owner can delete this list.");

        var list = await _context.TodoLists.FindAsync(listId);
        _context.TodoLists.Remove(list!);
        await _context.SaveChangesAsync();
    }

    private async Task<string?> GetUserRoleAsync(Guid listId, Guid userId)
    {
        var list = await _context.TodoLists.FindAsync(listId);
        if (list == null) return null;
        if (list.OwnerId == userId) return "Owner";

        var permission = await _context.ListPermissions
            .SingleOrDefaultAsync(lp => lp.TodoListId == listId && lp.UserId == userId);

        return permission?.Role.ToString();
    }

    private static TodoListResponse ToResponse(TodoList list, string role) => new()
    {
        Id = list.Id,
        Name = list.Name,
        Role = role,
        CreatedAt = list.CreatedAt,
        UpdatedAt = list.UpdatedAt
    };
}
