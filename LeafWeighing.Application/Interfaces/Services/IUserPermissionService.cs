using LeafWeighing.Application.DTOs.Permissions;

namespace LeafWeighing.Application.Interfaces.Services;

public interface IUserPermissionService
{
    Task<IEnumerable<UserPermissionDto>> GetUsersAsync();
    Task<UserPermissionDto?> UpdateAsync(int userInd, UpdateUserPermissionRequestDto request);
}
