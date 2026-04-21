using LeafWeighing.Application.DTOs.Deduction;

namespace LeafWeighing.Application.Interfaces.Services;

public interface IDeductionService
{
    Task<DeductionSummaryDto> GetDeductionSummaryAsync(int regNo, string leafType);
    Task<object> SaveDeductionAsync(SaveDeductionRequestDto request);
    Task<IEnumerable<TransactionDto>> GetTodayTransactionsAsync(int regNo);
}