using LeafWeighing.Application.DTOs.Common;
using LeafWeighing.Application.DTOs.Permissions;
using LeafWeighing.Application.Interfaces.Services;
using LeafWeighing.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeafWeighing.Api.Controllers;

[ApiController]
[Route("api/user-permissions")]
[Authorize(Policy = PermissionPolicies.AdminOnly)]
public class UserPermissionsController : ControllerBase
{
    private readonly IUserPermissionService _permissionService;

    public UserPermissionsController(IUserPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<UserPermissionDto>>>> GetUsers()
    {
        var users = await _permissionService.GetUsersAsync();
        return Ok(ApiResponse<IEnumerable<UserPermissionDto>>.Ok(users));
    }

    [HttpPut("{userInd:int}")]
    public async Task<ActionResult<ApiResponse<UserPermissionDto>>> Update(
        int userInd,
        [FromBody] UpdateUserPermissionRequestDto request)
    {
        var user = await _permissionService.UpdateAsync(userInd, request);
        if (user == null)
        {
            return NotFound(ApiResponse<UserPermissionDto>.Fail("User not found"));
        }

        return Ok(ApiResponse<UserPermissionDto>.Ok(user, "Permissions updated successfully"));
    }
}
