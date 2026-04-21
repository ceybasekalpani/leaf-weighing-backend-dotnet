using LeafWeighing.Application.DTOs.Collection;

namespace LeafWeighing.Application.Interfaces.Services;

public interface ICollectionService
{
    Task<IEnumerable<CollectionSummaryDto>> GetTodayCollectionsAsync();
    Task<IEnumerable<CollectionSummaryDto>> GetCollectionsByDateAsync(DateTime date);
    Task<IEnumerable<CollectionDetailDto>> GetCollectionDetailsAsync(int regNo);
}