using LeafWeighing.Domain.Entities;

namespace LeafWeighing.Application.Interfaces.Repositories;

public interface ITrLeafCollectionRepository : IGenericRepository<TrLeafCollectionTemp>
{
    Task<IEnumerable<IGrouping<int, TrLeafCollectionTemp>>> GetTodayGroupedCollectionsAsync();
    Task<IEnumerable<IGrouping<int, TrLeafCollectionTemp>>> GetGroupedCollectionsByDateAsync(DateTime date);
    Task<TrLeafCollectionTemp?> GetSupplierByRegNoAsync(int regNo);
    Task<IEnumerable<TrLeafCollectionTemp>> SearchSuppliersAsync(string query);
    Task<object?> GetTodayDeductionSummaryAsync(int regNo, string leafType);
    Task<IEnumerable<TrLeafCollectionTemp>> GetTodayTransactionsAsync(int regNo);
    Task<TrLeafCollectionTemp> AddDeductionAsync(TrLeafCollectionTemp deduction);
    Task<decimal> GetRouteTotalWeightAsync(string routeName, int date, string month);
}