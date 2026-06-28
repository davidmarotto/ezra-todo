using System.ComponentModel.DataAnnotations;

namespace TodoApi.DTOs.TodoLists;

public class CreateTodoListRequest
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
}
