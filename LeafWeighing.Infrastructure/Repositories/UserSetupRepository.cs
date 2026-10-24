using Microsoft.EntityFrameworkCore;
using LeafWeighing.Domain.Entities;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Infrastructure.Data;

namespace LeafWeighing.Infrastructure.Repositories;

public class UserSetupRepository : GenericRepository<UserSetup>, IUserSetupRepository
{
    private readonly SetupDbContext _setupContext;

    public UserSetupRepository(SetupDbContext context) : base(context)
    {
        _setupContext = context;
    }

    public async Task<UserSetup?> GetUserByCredentialsAsync(string username, string password)
    {
        var loginIdentifier = username.Trim();
        var users = _setupContext.UserSetup
            .AsNoTracking()
            .Where(x => x.Password == password && x.Active == true);

        if (int.TryParse(loginIdentifier, out var userId))
        {
            return await users.FirstOrDefaultAsync(x => x.Ind == userId);
        }

        return await users.FirstOrDefaultAsync(x => x.UserName == loginIdentifier);
    }
}
