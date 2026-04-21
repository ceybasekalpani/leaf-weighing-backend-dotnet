using LeafWeighing.Application.DTOs.LeafCount;

namespace LeafWeighing.Application.Interfaces.Services;

public interface ILeafCountService
{
    Task<IEnumerable<string>> GetDistinctRoutesAsync();
    Task<RouteTotalWeightDto> GetRouteTotalWeightAsync(string routeName, int date, string month);
    Task<object> SaveLeafCountAsync(LeafCountRequestDto request);
    Task<IEnumerable<LeafCountResponseDto>> GetLeafCountHistoryAsync(string? month, string? route, DateTime? startDate, DateTime? endDate);
}