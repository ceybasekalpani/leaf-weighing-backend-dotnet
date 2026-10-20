using LeafWeighing.Application.DTOs.Permissions;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Application.Interfaces.Services;
using LeafWeighing.Domain.Entities;

namespace LeafWeighing.Application.Services;

public class UserPermissionService : IUserPermissionService
{
    private readonly IUserPermissionRepository _permissionRepository;

    public UserPermissionService(IUserPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<IEnumerable<UserPermissionDto>> GetUsersAsync()
    {
        var users = await _permissionRepository.GetUsersWithPermissionsAsync();
        return users.Select(Map);
    }

    public async Task<UserPermissionDto?> UpdateAsync(int userInd, UpdateUserPermissionRequestDto request)
    {
        var user = await _permissionRepository.GetUserWithPermissionsAsync(userInd);
        if (user == null)
        {
            return null;
        }

        await _permissionRepository.UpsertAsync(userInd, request.IsCanAddDeduction, request.IsCanAddCount);
        user.AppPermissions = new UserPermission
        {
            UserInd = userInd,
            IsCanAddDeduction = request.IsCanAddDeduction,
            IsCanAddCount = request.IsCanAddCount
        };

        return Map(user);
    }

    private static UserPermissionDto Map(UserSetup user)
    {
        return new UserPermissionDto
        {
            UserInd = user.Ind,
            UserName = user.UserName,
            FullName = user.FullName,
            Active = user.Active == true,
            IsCanAddDeduction = user.AppPermissions?.IsCanAddDeduction ?? true,
            IsCanAddCount = user.AppPermissions?.IsCanAddCount ?? true
        };
    }
}
