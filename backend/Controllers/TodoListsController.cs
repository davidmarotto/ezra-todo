using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs.TodoLists;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("lists")]
[Authorize]
public class TodoListsController : ControllerBase
{
    private readonly ITodoListService _todoListService;

    public TodoListsController(ITodoListService todoListService)
    {
        _todoListService = todoListService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLists()
    {
        var lists = await _todoListService.GetListsForUserAsync(GetUserId());
        return Ok(lists);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetList(Guid id)
    {
        try
        {
            var list = await _todoListService.GetListAsync(id, GetUserId());
            return Ok(list);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateList(CreateTodoListRequest request)
    {
        var list = await _todoListService.CreateListAsync(request, GetUserId());
        return CreatedAtAction(nameof(GetList), new { id = list.Id }, list);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateList(Guid id, UpdateTodoListRequest request)
    {
        try
        {
            var list = await _todoListService.UpdateListAsync(id, request, GetUserId());
            return Ok(list);
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
    public async Task<IActionResult> DeleteList(Guid id)
    {
        try
        {
            await _todoListService.DeleteListAsync(id, GetUserId());
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
