using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs.ListPermissions;
using TodoApi.Models;

namespace TodoApi.Services;

public class ListPermissionService : IListPermissionService
{
    private readonly AppDbContext _context;

    public ListPermissionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ListPermissionResponse> ShareListAsync(Guid todoListId, Guid requestingUserId, ShareListRequest request)
    {
        var list = await _context.TodoLists.FindAsync(todoListId);
        if (list == null || list.OwnerId != requestingUserId)
            throw new UnauthorizedAccessException("List can only be shared by the ownwer.");

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (targetUser == null)
            throw new KeyNotFoundException("No user found with that email.");

        if (targetUser.Id == requestingUserId)
            throw new InvalidOperationException("Cannot share a list with yourself");

        var existing = await _context.ListPermissions
            .FirstOrDefaultAsync(p => p.UserId == targetUser.Id && p.TodoListId == todoListId);

        if (existing != null)
            throw new InvalidOperationException("User already has permission for this list.");

        var permission = new ListPermission
        {
            TodoListId = todoListId,
            UserId = targetUser.Id,
            Role = request.Role
        };

        _context.ListPermissions.Add(permission);
        await _context.SaveChangesAsync();

        return new ListPermissionResponse
        {
            UserId = targetUser.Id,
            Email = targetUser.Email,
            Role = permission.Role,
            InvitedAt = permission.InvitedAt
        };
    }

    public async Task RevokeAccessAsync(Guid todoListId, Guid requestingUserId, Guid targetUserId)
    {
        var list = await _context.TodoLists.FindAsync(todoListId);
        if (list == null || list.OwnerId != requestingUserId)
            throw new UnauthorizedAccessException("Only the list owner can revoke access.");

        var permission = await _context.ListPermissions
            .FirstOrDefaultAsync(p => p.TodoListId == list.Id && p.UserId == targetUserId);
        if (permission == null)
            throw new KeyNotFoundException("No permission found for user.");

        _context.ListPermissions.Remove(permission);

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ListPermissionResponse>> GetPermissionsAysnc(Guid todoListId, Guid requestingUserId)
    {
        var list = await _context.TodoLists.FindAsync(todoListId);
        if (list == null || list.OwnerId != requestingUserId)
            throw new UnauthorizedAccessException("Only the list owner can view permissions.");

        return await _context.ListPermissions
            .Include(p => p.User)
            .Where(p => p.TodoListId == todoListId)
            .Select(p => new ListPermissionResponse
            {
                UserId = p.UserId,
                Email = p.User.Email,
                Role = p.Role,
                InvitedAt = p.InvitedAt
            })
            .ToListAsync();
        }
}
