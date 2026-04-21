using LeafWeighing.Application.DTOs.Collection;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Application.Interfaces.Services;
using LeafWeighing.Domain.Entities;

namespace LeafWeighing.Application.Services;

public class CollectionService : ICollectionService
{
    private readonly ITrLeafCollectionRepository _collectionRepository;

    public CollectionService(ITrLeafCollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository;
    }

    public async Task<IEnumerable<CollectionSummaryDto>> GetTodayCollectionsAsync()
    {
        var today = DateTime.Today;
        var grouped = await _collectionRepository.GetTodayGroupedCollectionsAsync();
        var result = new List<CollectionSummaryDto>();

        foreach (var group in grouped)
        {
            var regNo = group.Key;
            var items = group.ToList();

            var summary = new CollectionSummaryDto
            {
                RegNo = regNo,
                SupplierName = items.First().Dealer ?? $"Supplier {regNo}",
                Route = items.First().Route ?? "",
                // Fix Ambiguity by casting to (double)
                Bags = Math.Round((double)items.Where(x => x.IsDeduction == false).Sum(x => x.Qty ?? 0)).ToString(),
                Gross = Math.Round((double)items.Where(x => x.IsDeduction == false).Sum(x => x.Gross ?? 0)).ToString(),
                TotalBagWeight = Math.Round((double)items.Where(x => x.IsDeduction == true).Sum(x => x.BagWeight ?? 0)).ToString(),
                TotalCoarce = Math.Round((double)items.Where(x => x.IsDeduction == true).Sum(x => x.Coarse ?? 0)).ToString(),
                TotalWater = Math.Round((double)items.Where(x => x.IsDeduction == true).Sum(x => x.Water ?? 0)).ToString(),
                TotalBoiled = Math.Round((double)items.Where(x => x.IsDeduction == true).Sum(x => x.Boild ?? 0)).ToString(),
                TotalRejected = Math.Round((double)items.Where(x => x.IsDeduction == true).Sum(x => x.Rejected ?? 0)).ToString(),
                NetWeight = Math.Round((double)items.Where(x => x.IsDeduction == false).Sum(x => x.NetWeight ?? 0)).ToString(),

                TransactionCount = items.Count,
                CollectionCount = items.Count(x => x.IsDeduction == false),
                DeductionCount = items.Count(x => x.IsDeduction == true),
                Date = today.ToString("yyyy-MM-dd"),
                Month = today.ToString("MMM-yyyy")
            };
            result.Add(summary);
        }
        return result.OrderByDescending(x => x.RegNo);
    }

    public async Task<IEnumerable<CollectionSummaryDto>> GetCollectionsByDateAsync(DateTime date)
    {
        var grouped = await _collectionRepository.GetGroupedCollectionsByDateAsync(date);
        var result = new List<CollectionSummaryDto>();

        foreach (var group in grouped)
        {
            var regNo = group.Key;
            var items = group.ToList();

            var summary = new CollectionSummaryDto
            {
                RegNo = regNo,
                SupplierName = items.First().Dealer ?? $"Supplier {regNo}",
                Route = items.First().Route ?? "",
                Bags = Math.Round((double)items.Where(x => x.IsDeduction == false).Sum(x => x.Qty ?? 0)).ToString(),
                Gross = Math.Round((double)items.Where(x => x.IsDeduction == false).Sum(x => x.Gross ?? 0)).ToString(),
                TotalBagWeight = Math.Round((double)items.Where(x => x.IsDeduction == true).Sum(x => x.BagWeight ?? 0)).ToString(),
                TotalCoarce = Math.Round((double)items.Where(x => x.IsDeduction == true).Sum(x => x.Coarse ?? 0)).ToString(),
                TotalWater = Math.Round((double)items.Where(x => x.IsDeduction == true).Sum(x => x.Water ?? 0)).ToString(),
                TotalBoiled = Math.Round((double)items.Where(x => x.IsDeduction == true).Sum(x => x.Boild ?? 0)).ToString(),
                TotalRejected = Math.Round((double)items.Where(x => x.IsDeduction == true).Sum(x => x.Rejected ?? 0)).ToString(),
                NetWeight = Math.Round((double)items.Where(x => x.IsDeduction == false).Sum(x => x.NetWeight ?? 0)).ToString(),

                TransactionCount = items.Count,
                CollectionCount = items.Count(x => x.IsDeduction == false),
                DeductionCount = items.Count(x => x.IsDeduction == true),
                // Use 'date' (parameter) instead of 'today'
                Date = date.ToString("yyyy-MM-dd"),
                Month = date.ToString("MMM-yyyy")
            };
            result.Add(summary);
        }
        return result.OrderByDescending(x => x.RegNo);
    }

    public async Task<IEnumerable<CollectionDetailDto>> GetCollectionDetailsAsync(int regNo)
    {
        var items = await _collectionRepository.FindAsync(x => x.RegNo == regNo);

        return items.OrderByDescending(x => x.LogTime).Select(x => new CollectionDetailDto
        {
            Ind = x.Ind,
            RegNo = x.RegNo,
            SupplierName = x.Dealer,
            Route = x.Route,
            LeafType = x.LeafType,
            Bags = x.Qty,
            Gross = x.Gross,
            BagWeight = x.BagWeight,
            Water = x.Water,
            Coarce = x.Coarse,
            Rejected = x.Rejected,
            Boiled = x.Boild,
            NetWeight = x.NetWeight,
            Shift = x.Shift,
            UserName = x.UserName,
            Mode = x.Mode,
            LogTime = x.LogTime,
            DisplayDate = x.LogTime?.ToString("dd/MM/yyyy"),
            DisplayTime = x.LogTime?.ToString("hh:mm tt"),
            Source = x.Mode == "App" ? "Mobile App" : "Web System"
        });
    }
}