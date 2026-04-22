using Microsoft.EntityFrameworkCore;
using LeafWeighing.Domain.Entities;
using LeafWeighing.Application.DTOs.Deduction;
using LeafWeighing.Application.DTOs.Supplier;
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

    public async Task<SupplierDto?> GetSupplierByRegNoAsync(int regNo)
    {
        return await _context.TrLeafCollectionTemp
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
        return await _context.TrLeafCollectionTemp
            .Where(x => x.RegNo.ToString().Contains(query) ||
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

        var items = await _context.TrLeafCollectionTemp
            .Where(x => x.RegNo == regNo &&
                       x.LeafType == leafType &&
                       x.LogTime.HasValue &&
                       x.LogTime.Value.Date == today)
            .ToListAsync();

        if (!items.Any()) return new DeductionSummaryDto();

        return new DeductionSummaryDto
        {
            TotalBags = items.Where(x => x.IsDeduction == false).Sum(x => x.Qty ?? 0),
            TotalGross = items.Where(x => x.IsDeduction == false).Sum(x => x.Gross ?? 0),
            TotalBagWeight = items.Where(x => x.IsDeduction == true).Sum(x => x.BagWeight ?? 0),
            TotalCoarse = items.Where(x => x.IsDeduction == true).Sum(x => x.Coarse ?? 0),
            TotalWater = items.Where(x => x.IsDeduction == true).Sum(x => x.Water ?? 0),
            TotalBoiled = items.Where(x => x.IsDeduction == true).Sum(x => x.Boild ?? 0),
            TotalRejected = items.Where(x => x.IsDeduction == true).Sum(x => x.Rejected ?? 0),
            TotalNetWeight = items.Where(x => x.IsDeduction == false).Sum(x => x.NetWeight ?? 0),
            TransactionCount = items.Count(x => x.IsDeduction == true)
        };
    }

    public async Task<IEnumerable<TransactionDto>> GetTodayTransactionsAsync(int regNo)
    {
        var today = DateTime.Today;
        return await _context.TrLeafCollectionTemp
            .Where(x => x.RegNo == regNo &&
                       x.LogTime.HasValue &&
                       x.LogTime.Value.Date == today &&
                       x.IsDeduction == true)
            .OrderByDescending(x => x.LogTime)
            .Select(x => new TransactionDto
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
                LogTime = x.LogTime
            })
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

        var items = await _context.TrLeafCollectionTemp
            .Where(x => x.Route != null && x.Route.Trim() == trimmedRoute &&
                       x.LogTime.HasValue && x.LogTime.Value.Date == targetDate)
            .ToListAsync();

        if (!items.Any()) return 0;

        var totalGross = items.Sum(x => x.Gross ?? 0);
        var totalDeductions = items.Sum(x =>
            (x.Coarse ?? 0) + (x.Water ?? 0) + (x.BagWeight ?? 0) +
            (x.Spd ?? 0) + (x.Boild ?? 0) + (x.Rejected ?? 0) +
            (x.RouteDeduct ?? 0) + (x.ExcessLeaf ?? 0) +
            (x.Transfer ?? 0) + (x.RouteDeductPre ?? 0));

        var netWeight = totalGross - totalDeductions;
        return netWeight > 0 ? netWeight : 0;
    }
}