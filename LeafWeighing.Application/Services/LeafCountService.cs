using LeafWeighing.Domain.Entities;
using LeafWeighing.Application.DTOs.LeafCount;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Application.Interfaces.Services;

namespace LeafWeighing.Application.Services;

public class LeafCountService : ILeafCountService
{
    private readonly IRegLeafCountRepository _leafCountRepository;
    private readonly ITrLeafCollectionRepository _collectionRepository;

    public LeafCountService(IRegLeafCountRepository leafCountRepository, ITrLeafCollectionRepository collectionRepository)
    {
        _leafCountRepository = leafCountRepository;
        _collectionRepository = collectionRepository;
    }

    // LeafCountService.cs - Update GetDistinctRoutesAsync method
public async Task<IEnumerable<string>> GetDistinctRoutesAsync()
{
    // Get routes from leaf counts (existing functionality)
    var leafCountRoutes = await _leafCountRepository.GetDistinctRoutesAsync();
    
    // Get routes from deductions (new functionality)
    var deductionRoutes = await _collectionRepository.GetDistinctRoutesFromDeductionsAsync();
    
    // Combine and return distinct routes from both sources
    var allRoutes = leafCountRoutes
        .Union(deductionRoutes)
        .Where(r => !string.IsNullOrWhiteSpace(r))
        .OrderBy(r => r)
        .ToList();
    
    return allRoutes;
}
    public async Task<RouteTotalWeightDto> GetRouteTotalWeightAsync(string routeName, int date, string month)
    {
        var totalWeight = await _collectionRepository.GetRouteTotalWeightAsync(routeName, date, month);

        return new RouteTotalWeightDto
        {
            Route = routeName,
            Date = date,
            Month = month,
            TotalWeight = totalWeight
        };
    }

    public async Task<object> SaveLeafCountAsync(LeafCountRequestDto request)
    {
        var leafCount = new RegLeafCount
        {
            Date = request.Date,
            Month = request.Month,
            Route = request.Route,
            BestLeaf = request.BestLeaf,
            BelowBest = request.BellowBest,
            Poor = request.Poor,
            User = request.UserName ?? "mobile_user",
            LogTime = DateTime.Now,
            PcName = request.PcName ?? Environment.MachineName
        };

        var saved = await _leafCountRepository.AddLeafCountAsync(leafCount);

        return new { success = true, ind = saved.Ind, message = "Leaf count saved successfully" };
    }

    public async Task<IEnumerable<LeafCountResponseDto>> GetLeafCountHistoryAsync(string? month, string? route, DateTime? startDate, DateTime? endDate)
    {
        var history = await _leafCountRepository.GetLeafCountHistoryAsync(month, route, startDate, endDate);

        return history.Select(x => new LeafCountResponseDto
        {
            Ind = x.Ind,
            Date = x.Date,
            Month = x.Month,
            Route = x.Route,
            BestLeaf = x.BestLeaf,
            BellowBest = x.BelowBest,
            Poor = x.Poor,
            User = x.User,
            LogTime = x.LogTime,
            PcName = x.PcName
        });
    }
}