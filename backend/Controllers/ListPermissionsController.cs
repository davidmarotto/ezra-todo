using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs.ListPermissions;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("lists/{listId}/permissions")]
[Authorize]
public class ListPermissionsController : ControllerBase
{
    private readonly IListPermissionService _permissionService;

    public ListPermissionsController(IListPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpPost]
    public async Task<IActionResult> ShareList(Guid listId, [FromBody] ShareListRequest request)
    {
        try
        {
            var response = await _permissionService.ShareListAsync(listId, GetUserId(), request);
            return Ok(response);
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is UnauthorizedAccessException)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Title = ex.Message });
        }
    }

    [HttpGet] 
    public async Task<IActionResult> GetPermissionsAsync(Guid listId)
    {
        try
        {
            var permissions = await _permissionService.GetPermissionsAysnc(listId, GetUserId());
            return Ok(permissions);
        }
        catch(UnauthorizedAccessException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
    }

    [HttpDelete("{targetUserId}")]
    public async Task<IActionResult> RevokeListPermission(Guid listId, Guid targetUserId)
    {
        try
        {
            await _permissionService.RevokeAccessAsync(listId, GetUserId(), targetUserId);
            return NoContent();
        }
        catch(KeyNotFoundException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
        catch(UnauthorizedAccessException ex)
        {
            return NotFound(new ProblemDetails { Title = ex.Message });
        }
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}