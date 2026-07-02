using TodoApi.Models;

namespace TodoApi.DTOs.ListPermissions;

public class ShareListRequest
{
    public required string Email { get; set; }
    public required PermissionRole Role { get; set; }
}
