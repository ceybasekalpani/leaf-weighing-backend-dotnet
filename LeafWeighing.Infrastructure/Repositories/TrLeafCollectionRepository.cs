using Microsoft.EntityFrameworkCore;
using LeafWeighing.Domain.Entities;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Infrastructure.Data;

namespace LeafWeighing.Infrastructure.Repositories;

public class TrLeafCollectionRepository : GenericRepository<TrLeafCollectionTemp>, ITrLeafCollectionRepository
{
    private readonly BoughtLeafDbContext _context;

    public TrLeafCollectionRepository(BoughtLeafDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IGrouping<int, TrLeafCollectionTemp>>> GetTodayGroupedCollectionsAsync()
    {
        var today = DateTime.Today;
        var collections = await _context.TrLeafCollectionTemp
            .Where(x => x.LogTime.HasValue && x.LogTime.Value.Date == today)
            .ToListAsync();

        return collections.GroupBy(x => x.RegNo ?? 0);
    }

    public async Task<IEnumerable<IGrouping<int, TrLeafCollectionTemp>>> GetGroupedCollectionsByDateAsync(DateTime date)
    {
        var collections = await _context.TrLeafCollectionTemp
            .Where(x => x.LogTime.HasValue && x.LogTime.Value.Date == date)
            .ToListAsync();

        return collections.GroupBy(x => x.RegNo ?? 0);
    }

    public async Task<TrLeafCollectionTemp?> GetSupplierByRegNoAsync(int regNo)
    {
        return await _context.TrLeafCollectionTemp
            .Where(x => x.RegNo == regNo)
            .OrderByDescending(x => x.LogTime)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TrLeafCollectionTemp>> SearchSuppliersAsync(string query)
    {
        return await _context.TrLeafCollectionTemp
            .Where(x => x.RegNo.ToString().Contains(query) ||
                        (x.Dealer != null && x.Dealer.Contains(query)))
            .OrderBy(x => x.RegNo)
            .Take(50)
            .ToListAsync();
    }

    public async Task<object?> GetTodayDeductionSummaryAsync(int regNo, string leafType)
    {
        var today = DateTime.Today;

        var summary = await _context.TrLeafCollectionTemp
            .Where(x => x.RegNo == regNo &&
                       x.LeafType == leafType &&
                       x.LogTime.HasValue &&
                       x.LogTime.Value.Date == today)
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalBags = g.Where(x => x.IsDeduction == false).Sum(x => x.Qty ?? 0),
                TotalGross = g.Where(x => x.IsDeduction == false).Sum(x => x.Gross ?? 0),
                TotalBagWeight = g.Where(x => x.IsDeduction == true).Sum(x => x.BagWeight ?? 0),
                TotalCoarse = g.Where(x => x.IsDeduction == true).Sum(x => x.Coarse ?? 0),
                TotalWater = g.Where(x => x.IsDeduction == true).Sum(x => x.Water ?? 0),
                TotalBoiled = g.Where(x => x.IsDeduction == true).Sum(x => x.Boild ?? 0),
                TotalRejected = g.Where(x => x.IsDeduction == true).Sum(x => x.Rejected ?? 0),
                TotalNetWeight = g.Where(x => x.IsDeduction == false).Sum(x => x.NetWeight ?? 0),
                TransactionCount = g.Count(x => x.IsDeduction == true)
            })
            .FirstOrDefaultAsync();

        if (summary == null)
        {
            return new
            {
                TotalBags = 0,
                TotalGross = 0,
                TotalBagWeight = 0,
                TotalCoarse = 0,
                TotalWater = 0,
                TotalBoiled = 0,
                TotalRejected = 0,
                TotalNetWeight = 0,
                TransactionCount = 0
            };
        }

        return summary;
    }

    public async Task<IEnumerable<TrLeafCollectionTemp>> GetTodayTransactionsAsync(int regNo)
    {
        var today = DateTime.Today;
        return await _context.TrLeafCollectionTemp
            .Where(x => x.RegNo == regNo &&
                       x.LogTime.HasValue &&
                       x.LogTime.Value.Date == today &&
                       x.IsDeduction == true)
            .OrderByDescending(x => x.LogTime)
            .ToListAsync();
    }

    public async Task<TrLeafCollectionTemp> AddDeductionAsync(TrLeafCollectionTemp deduction)
    {
        deduction.IsDeduction = true;
        deduction.LogTime = DateTime.Now;
        deduction.Qty = 1;
        deduction.Gross = 0;
        deduction.NetWeight = 0;

        await _context.TrLeafCollectionTemp.AddAsync(deduction);
        await _context.SaveChangesAsync();
        return deduction;
    }

    public async Task<decimal> GetRouteTotalWeightAsync(string routeName, int date, string month)
    {
        var monthMap = new Dictionary<string, int>
        {
            { "Jan", 1 }, { "Feb", 2 }, { "Mar", 3 }, { "Apr", 4 },
            { "May", 5 }, { "Jun", 6 }, { "Jul", 7 }, { "Aug", 8 },
            { "Sep", 9 }, { "Oct", 10 }, { "Nov", 11 }, { "Dec", 12 }
        };

        var monthParts = month.Split('-');
        if (monthParts.Length != 2) return 0;

        var monthAbbr = monthParts[0];
        var year = monthParts[1];

        if (!monthMap.TryGetValue(monthAbbr, out var monthNumber)) return 0;

        var targetDate = new DateTime(int.Parse(year), monthNumber, date);
        var trimmedRoute = routeName.Trim();

        var result = await _context.TrLeafCollectionTemp
            .Where(x => x.Route != null && x.Route.Trim() == trimmedRoute &&
                       x.LogTime.HasValue && x.LogTime.Value.Date == targetDate)
            .GroupBy(x => 1)
            .Select(g => new
            {
                TotalGross = g.Sum(x => x.Gross ?? 0),
                TotalDeductions = g.Sum(x => (x.Coarse ?? 0) + (x.Water ?? 0) + (x.BagWeight ?? 0) +
                                              (x.Spd ?? 0) + (x.Boild ?? 0) + (x.Rejected ?? 0) +
                                              (x.RouteDeduct ?? 0) + (x.ExcessLeaf ?? 0) +
                                              (x.Transfer ?? 0) + (x.RouteDeductPre ?? 0))
            })
            .FirstOrDefaultAsync();

        if (result == null) return 0;

        var netWeight = result.TotalGross - result.TotalDeductions;
        return netWeight > 0 ? netWeight : 0;
    }
}