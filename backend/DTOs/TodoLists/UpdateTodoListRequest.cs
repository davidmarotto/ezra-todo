using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs.TodoLists;

public class UpdateTodoListRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
}
