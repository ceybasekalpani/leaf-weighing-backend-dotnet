using LeafWeighing.Domain.Entities;
using LeafWeighing.Application.DTOs.Deduction;
using LeafWeighing.Application.DTOs.Supplier;

namespace LeafWeighing.Application.Interfaces.Repositories;

public interface ITrLeafCollectionRepository : IGenericRepository<TrLeafCollectionTemp>
{
    Task<IEnumerable<IGrouping<int, TrLeafCollectionTemp>>> GetTodayGroupedCollectionsAsync();
    Task<IEnumerable<IGrouping<int, TrLeafCollectionTemp>>> GetGroupedCollectionsByDateAsync(DateTime date);
    Task<SupplierDto?> GetSupplierByRegNoAsync(int regNo);
    Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string query);
    Task<DeductionSummaryDto> GetTodayDeductionSummaryAsync(int regNo, string leafType);
    Task<IEnumerable<TransactionDto>> GetTodayTransactionsAsync(int regNo);
    Task<TrLeafCollectionTemp> AddDeductionAsync(TrLeafCollectionTemp deduction);
    Task<decimal> GetRouteTotalWeightAsync(string routeName, int date, string month);
    Task<IEnumerable<string>> GetDistinctRoutesFromDeductionsAsync();
}