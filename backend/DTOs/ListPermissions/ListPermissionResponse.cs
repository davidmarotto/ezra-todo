using TodoApi.Models;

namespace TodoApi.DTOs.ListPermissions;

public class ListPermissionResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public PermissionRole Role { get; set; }
    public DateTime InvitedAt { get; set; }
}
