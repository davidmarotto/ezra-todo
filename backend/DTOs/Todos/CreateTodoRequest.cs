using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs.Todos;

public class CreateTodoRequest
{
    [Required]
    [MaxLength(200)]
    public required string Title { get; set; }
    public DateTime? DueDate { get; set; }
}
