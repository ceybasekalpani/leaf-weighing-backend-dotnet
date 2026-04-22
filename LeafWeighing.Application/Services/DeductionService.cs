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
        return await _collectionRepository.GetTodayDeductionSummaryAsync(regNo, leafType);
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
            LogTime = DateTime.Now,
            Spd = 0,
            RouteDeduct = 0,
            ExcessLeaf = 0,
            Transfer = 0,
            RouteDeductPre = 0  // INT columns in DB — must be set to avoid NOT NULL violation
        };

        var saved = await _collectionRepository.AddDeductionAsync(deduction);

        return new { success = true, ind = saved.Ind, message = "Deduction saved successfully" };
    }

    public async Task<IEnumerable<TransactionDto>> GetTodayTransactionsAsync(int regNo)
    {
        return await _collectionRepository.GetTodayTransactionsAsync(regNo);
    }
}