using LeafWeighing.Domain.Entities;

namespace LeafWeighing.Application.Interfaces.Repositories;

public interface IRegLeafCountRepository : IGenericRepository<RegLeafCount>
{
    Task<IEnumerable<string>> GetDistinctRoutesAsync();
    Task<RegLeafCount> AddLeafCountAsync(RegLeafCount leafCount);
    Task<IEnumerable<RegLeafCount>> GetLeafCountHistoryAsync(string? month, string? route, DateTime? startDate, DateTime? endDate);
}