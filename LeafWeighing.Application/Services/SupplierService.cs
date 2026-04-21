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
        var supplier = await _collectionRepository.GetSupplierByRegNoAsync(regNo);

        if (supplier == null) return null;

        return new SupplierDto
        {
            RegNo = supplier.RegNo ?? 0,
            SupplierName = supplier.Dealer,
            Route = supplier.Route
        };
    }

    public async Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string query)
    {
        var suppliers = await _collectionRepository.SearchSuppliersAsync(query);

        return suppliers.Select(x => new SupplierDto
        {
            RegNo = x.RegNo ?? 0,
            SupplierName = x.Dealer,
            Route = x.Route
        }).DistinctBy(x => x.RegNo);
    }
}