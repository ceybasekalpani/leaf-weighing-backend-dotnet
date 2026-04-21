using Microsoft.EntityFrameworkCore;
using LeafWeighing.Domain.Entities;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Infrastructure.Data;

namespace LeafWeighing.Infrastructure.Repositories;

public class UserSetupRepository : GenericRepository<UserSetup>, IUserSetupRepository
{
    private readonly SetupDbContext _context;

    public UserSetupRepository(SetupDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<UserSetup?> GetUserByCredentialsAsync(string username, string password)
    {
        return await _context.UserSetup
            .FirstOrDefaultAsync(x => x.UserName == username &&
                                     x.Password == password &&
                                     x.Active == true);
    }
}