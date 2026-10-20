using LeafWeighing.Domain.Entities;

namespace LeafWeighing.Application.Interfaces.Repositories;

public interface IUserPermissionRepository
{
    Task<IEnumerable<UserSetup>> GetUsersWithPermissionsAsync();
    Task<UserSetup?> GetUserWithPermissionsAsync(int userInd);
    Task UpsertAsync(int userInd, bool isCanAddDeduction, bool isCanAddCount);
}
