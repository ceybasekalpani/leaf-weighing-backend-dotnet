using LeafWeighing.Domain.Entities;
using LeafWeighing.Application.DTOs.Deduction;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Application.Interfaces.Services;

namespace LeafWeighing.Application.Services;

public class DeductionService : IDeductionService
{
    private readonly ITrLeafCollectionRepository _collectionRepository;

    public DeductionService(ITrLeafCollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository;
    }

    public async Task<DeductionSummaryDto> GetDeductionSummaryAsync(int regNo, string leafType)
    {
        var summary = await _collectionRepository.GetTodayDeductionSummaryAsync(regNo, leafType);

        if (summary == null)
        {
            return new DeductionSummaryDto
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

        var summaryDict = summary as dynamic;

        return new DeductionSummaryDto
        {
            TotalBags = Convert.ToDecimal(summaryDict?.TotalBags ?? 0),
            TotalGross = Convert.ToDecimal(summaryDict?.TotalGross ?? 0),
            TotalBagWeight = Convert.ToDecimal(summaryDict?.TotalBagWeight ?? 0),
            TotalCoarse = Convert.ToDecimal(summaryDict?.TotalCoarse ?? 0),
            TotalWater = Convert.ToDecimal(summaryDict?.TotalWater ?? 0),
            TotalBoiled = Convert.ToDecimal(summaryDict?.TotalBoiled ?? 0),
            TotalRejected = Convert.ToDecimal(summaryDict?.TotalRejected ?? 0),
            TotalNetWeight = Convert.ToDecimal(summaryDict?.TotalNetWeight ?? 0),
            TransactionCount = Convert.ToInt32(summaryDict?.TransactionCount ?? 0)
        };
    }

    public async Task<object> SaveDeductionAsync(SaveDeductionRequestDto request)
    {
        var currentHour = DateTime.Now.Hour;
        var shift = currentHour < 12 ? "Morning" : "Evening";
        var dayNo = DateTime.Now.Day;
        var monthName = request.Month ?? DateTime.Now.ToString("MMM-yyyy");
        var mode = request.Mode ?? "App";
        var pcName = request.PcName ?? "MOBILE_APP";

        var deduction = new TrLeafCollectionTemp
        {
            RegNo = request.RegNo,
            Dealer = request.SupplierName,
            Route = request.Route,
            LeafType = request.LeafType ?? "Normal",
            Qty = 1,
            Gross = 0,
            BagWeight = request.BagWeight,
            Water = request.Water,
            Coarse = request.Coarse,
            Rejected = request.Rejected,
            Boild = request.Boiled,
            NetWeight = 0,
            Shift = shift,
            UserName = request.UserName ?? "mobile_user",
            Mode = mode,
            PcName = pcName,
            IsDeduction = true,
            MonthName = monthName,
            DayNo = dayNo,
            LogTime = DateTime.Now
        };

        var saved = await _collectionRepository.AddDeductionAsync(deduction);

        return new { success = true, ind = saved.Ind, message = "Deduction saved successfully" };
    }

    public async Task<IEnumerable<TransactionDto>> GetTodayTransactionsAsync(int regNo)
    {
        var transactions = await _collectionRepository.GetTodayTransactionsAsync(regNo);

        return transactions.Select(x => new TransactionDto
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
        });
    }
}