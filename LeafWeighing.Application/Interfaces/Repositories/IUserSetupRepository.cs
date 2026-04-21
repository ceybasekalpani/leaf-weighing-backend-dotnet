using LeafWeighing.Domain.Entities;

namespace LeafWeighing.Application.Interfaces.Repositories;

public interface IUserSetupRepository : IGenericRepository<UserSetup>
{
    Task<UserSetup?> GetUserByCredentialsAsync(string username, string password);
}