namespace TodoApi.Models;

public enum PermissionRole
{
    Editor,
    Viewer
}

public class ListPermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TodoListId { get; set; }
    public TodoList TodoList { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public PermissionRole Role { get; set; }
    public DateTime InvitedAt { get; set; } = DateTime.UtcNow;
}
