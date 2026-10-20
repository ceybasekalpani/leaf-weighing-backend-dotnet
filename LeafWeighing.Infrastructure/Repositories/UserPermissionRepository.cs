using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Domain.Entities;
using LeafWeighing.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeafWeighing.Infrastructure.Repositories;

public class UserPermissionRepository : IUserPermissionRepository
{
    private readonly SetupDbContext _context;

    public UserPermissionRepository(SetupDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<UserSetup>> GetUsersWithPermissionsAsync()
    {
        return await _context.UserSetup
            .AsNoTracking()
            .Include(user => user.AppPermissions)
            .OrderBy(user => user.UserName)
            .ToListAsync();
    }

    public async Task<UserSetup?> GetUserWithPermissionsAsync(int userInd)
    {
        return await _context.UserSetup
            .AsNoTracking()
            .Include(user => user.AppPermissions)
            .SingleOrDefaultAsync(user => user.Ind == userInd);
    }

    public async Task UpsertAsync(int userInd, bool isCanAddDeduction, bool isCanAddCount)
    {
        var permissions = await _context.UserPermissions.FindAsync(userInd);

        if (permissions == null)
        {
            permissions = new UserPermission { UserInd = userInd };
            await _context.UserPermissions.AddAsync(permissions);
        }

        permissions.IsCanAddDeduction = isCanAddDeduction;
        permissions.IsCanAddCount = isCanAddCount;
        await _context.SaveChangesAsync();
    }
}
