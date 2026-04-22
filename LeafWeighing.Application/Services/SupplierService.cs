using LeafWeighing.Application.DTOs.Supplier;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Application.Interfaces.Services;

namespace LeafWeighing.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly ITrLeafCollectionRepository _collectionRepository;

    public SupplierService(ITrLeafCollectionRepository collectionRepository)
    {
        _collectionRepository = collectionRepository;
    }

    public async Task<SupplierDto?> GetSupplierByRegNoAsync(int regNo)
    {
        return await _collectionRepository.GetSupplierByRegNoAsync(regNo);
    }

    public async Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string query)
    {
        return await _collectionRepository.SearchSuppliersAsync(query);
    }
}