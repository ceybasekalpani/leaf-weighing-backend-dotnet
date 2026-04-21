using LeafWeighing.Application.DTOs.Supplier;

namespace LeafWeighing.Application.Interfaces.Services;

public interface ISupplierService
{
    Task<SupplierDto?> GetSupplierByRegNoAsync(int regNo);
    Task<IEnumerable<SupplierDto>> SearchSuppliersAsync(string query);
}