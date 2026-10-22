using LeafWeighing.Domain.Calculations;
using Microsoft.EntityFrameworkCore;
using LeafWeighing.Domain.Entities;
using LeafWeighing.Application.DTOs.Deduction;
using LeafWeighing.Application.DTOs.Supplier;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Infrastructure.Data;

namespace LeafWeighing.Infrastructure.Repositories;

public class TrLeafCollectionRepository : GenericRepository<TrLeafCollectionTemp>, ITrLeafCollectionRepository
{
    private readonly BoughtLeafDbContext _boughtLeafContext;

    public TrLeafCollectionRepository(BoughtLeafDbContext context) : base(context)
    {
        _boughtLeafContext = context;
    }

    public async Task<IEnumerable<IGrouping<int, TrLeafCollectionTemp>>> GetTodayGroupedCollectionsAsync()
    {
        var today = DateTime.Today;
        var collections = await _boughtLeafContext.TrLeafCollectionTemp
            .Where(x => x.LogTime.HasValue && x.LogTime.Value.Date == today)
            .ToListAsync();

        return collections.GroupBy(x => x.RegNo ?? 0);
    }

    public async Task<IEnumerable<IGrouping<int, TrLeafCollectionTemp>>> GetGroupedCollectionsByDateAsync(DateTime date)
    {
        var collections = await _boughtLeafContext.TrLeafCollectionTemp
            .Where(x => x.LogTime.HasValue && x.LogTime.Value.Date == date)
            .ToListAsync();

        return collections.GroupBy(x => x.RegNo ?? 0);
    }

    public async Task<SupplierDto?> GetSupplierByRegNoAsync(int regNo)
    {
        return await _boughtLeafContext.TrLeafCollectionTemp
            .Where(x => x.RegNo == regNo)
            .OrderByDescending(x => x.LogTime)
            .Select(x => new SupplierDto
            {
                RegNo = x.RegNo ?? 0,
                SupplierName = x.Dealer,
                Route = x.Route
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string query)
    {
        return await _boughtLeafContext.TrLeafCollectionTemp
            .Where(x => (x.RegNo.HasValue && x.RegNo.Value.ToString().Contains(query)) ||
                        (x.Dealer != null && x.Dealer.Contains(query)))
            .OrderBy(x => x.RegNo)
            .Select(x => new SupplierDto
            {
                RegNo = x.RegNo ?? 0,
                SupplierName = x.Dealer,
                Route = x.Route
            })
            .Distinct()
            .Take(50)
            .ToListAsync();
    }

    public async Task<DeductionSummaryDto> GetTodayDeductionSummaryAsync(int regNo, string leafType)
    {
        var today = DateTime.Today;

        var items = await _boughtLeafContext.TrLeafCollectionTemp
            .Where(x => x.RegNo == regNo &&
                       x.LeafType == leafType &&
                       x.LogTime.HasValue &&
                       x.LogTime.Value.Date == today)
            .ToListAsync();

        if (!items.Any()) return new DeductionSummaryDto();

        return new DeductionSummaryDto
        {
            TotalBags = items.Where(x => x.IsDeduction == false).Sum(x => x.Qty ?? 0),
            TotalRawGross = items.Sum(x => x.Gross ?? 0),
            TotalGross = LeafWeightCalculator.CalculateAdjustedGrossTotal(items),
            TotalBagWeight = items.Sum(x => x.BagWeight ?? 0),
            TotalCoarse = items.Sum(x => x.Coarse ?? 0),
            TotalWater = items.Sum(x => x.Water ?? 0),
            TotalBoiled = items.Sum(x => x.Boild ?? 0),
            TotalRejected = items.Sum(x => x.Rejected ?? 0),
            TotalSuperDeduction = items.Sum(x => x.SuperDeduction ?? 0),
            TotalNormalDeduction = items.Sum(x => x.NormalDeduction ?? 0),
            TotalNetWeight = LeafWeightCalculator.CalculateTotal(items),
            TransactionCount = items.Count(x => x.IsDeduction == true)
        };
    }

    public async Task<IEnumerable<TransactionDto>> GetTodayTransactionsAsync(int regNo)
    {
        var today = DateTime.Today;
        var items = await _boughtLeafContext.TrLeafCollectionTemp
            .Where(x => x.RegNo == regNo &&
                       x.LogTime.HasValue &&
                       x.LogTime.Value.Date == today &&
                       x.IsDeduction == true)
            .OrderByDescending(x => x.LogTime)
            .ToListAsync();

        return items.Select(x => new TransactionDto
            {
                Ind = x.Ind,
                RegNo = x.RegNo,
                SupplierName = x.Dealer,
                Route = x.Route,
                LeafType = x.LeafType,
                Bags = x.Qty,
                RawGross = x.Gross,
                Gross = LeafWeightCalculator.CalculateAdjustedGross(x),
                BagWeight = x.BagWeight,
                Water = x.Water,
                Coarce = x.Coarse,
                Rejected = x.Rejected,
                Boiled = x.Boild,
                SuperDeduction = x.SuperDeduction,
                NormalDeduction = x.NormalDeduction,
                NetWeight = LeafWeightCalculator.Calculate(x),
                Shift = x.Shift,
                UserName = x.UserName,
                LogTime = x.LogTime
            }).ToList();
    }

    public async Task<IEnumerable<string>> GetDistinctRoutesFromDeductionsAsync()
    {
        try
        {
            var routes = await _boughtLeafContext.TrLeafCollectionTemp
                .Where(x => x.Route != null && x.Route != "" && x.IsDeduction == true)
                .Select(x => x.Route)
                .Distinct()
                .OrderBy(r => r)
                .ToListAsync();

            return routes.OfType<string>().ToList();
        }
        catch (Exception)
        {
            return new List<string>();
        }
    }
    public async Task<TrLeafCollectionTemp> AddDeductionAsync(TrLeafCollectionTemp deduction)
    {
        deduction.IsDeduction = true;
        deduction.LogTime = DateTime.Now;
        deduction.Qty = 1;
        deduction.Gross = 0;
        deduction.NetWeight = LeafWeightCalculator.Calculate(deduction);

        await _boughtLeafContext.TrLeafCollectionTemp.AddAsync(deduction);
        await _boughtLeafContext.SaveChangesAsync();
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

        var items = await _boughtLeafContext.TrLeafCollectionTemp
            .Where(x => x.Route != null && x.Route.Trim() == trimmedRoute &&
                       x.LogTime.HasValue && x.LogTime.Value.Date == targetDate)
            .ToListAsync();

        if (!items.Any()) return 0;

        return LeafWeightCalculator.CalculateTotal(items);
    }
}
