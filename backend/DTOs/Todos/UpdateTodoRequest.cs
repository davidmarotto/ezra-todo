using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs.Todos;

public class UpdateTodoRequest
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
}
