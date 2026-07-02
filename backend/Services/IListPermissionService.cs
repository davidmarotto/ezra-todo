using TodoApi.DTOs.ListPermissions;

namespace TodoApi.Services;

public interface IListPermissionService
{
    Task<ListPermissionResponse> ShareListAsync(Guid todoListId, Guid requestingUserId, ShareListRequest request);
    Task RevokeAccessAsync(Guid todoListId, Guid requestingUserId, Guid targetUserId);
    Task<IEnumerable<ListPermissionResponse>> GetPermissionsAysnc(Guid todoListId, Guid requestingUserId);
}