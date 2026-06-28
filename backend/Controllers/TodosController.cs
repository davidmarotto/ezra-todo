using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs.Todos;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("lists/{listId}/todos")]
[Authorize]
public class TodosController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodosController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTodos(Guid listId, [FromQuery] string? status)
    {
        try
        {
            var todos = await _todoService.GetTodosAsync(listId, GetUserId(), status);
            return Ok(todos);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTodo(Guid listId, Guid id)
    {
        try
        {
            var todo = await _todoService.GetTodoAsync(listId, id, GetUserId());
            return Ok(todo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateTodo(Guid listId, CreateTodoRequest request)
    {
        try
        {
            var todo = await _todoService.CreateTodoAsync(listId, request, GetUserId());
            return CreatedAtAction(nameof(GetTodo), new { listId, id = todo.Id }, todo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ProblemDetails { Title = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTodo(Guid listId, Guid id, UpdateTodoRequest request)
    {
        try
        {
            var todo = await _todoService.UpdateTodoAsync(listId, id, request, GetUserId());
            return Ok(todo);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ProblemDetails { Title = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(Guid listId, Guid id)
    {
        try
        {
            await _todoService.DeleteTodoAsync(listId, id, GetUserId());
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, new ProblemDetails { Title = ex.Message });
        }
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
