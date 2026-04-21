using Microsoft.AspNetCore.Mvc;
using LeafWeighing.Application.DTOs.Common;
using LeafWeighing.Application.Interfaces.Services;

namespace LeafWeighing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;
    private readonly ILogger<SuppliersController> _logger;

    public SuppliersController(ISupplierService supplierService, ILogger<SuppliersController> logger)
    {
        _supplierService = supplierService;
        _logger = logger;
    }

    [HttpGet("{regNo}")]
    public async Task<ActionResult<ApiResponse<object>>> GetSupplierByRegNo(int regNo)
    {
        try
        {
            var supplier = await _supplierService.GetSupplierByRegNoAsync(regNo);

            if (supplier == null)
            {
                return NotFound(ApiResponse<object>.Fail("Supplier not found"));
            }

            return Ok(ApiResponse<object>.Ok(supplier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supplier for RegNo: {RegNo}", regNo);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error", ex.Message));
        }
    }

    [HttpGet("search/all")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> SearchSuppliers([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest(ApiResponse<IEnumerable<object>>.Fail("Search query is required"));
            }

            var suppliers = await _supplierService.SearchSuppliersAsync(query);
            return Ok(ApiResponse<IEnumerable<object>>.Ok(suppliers));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching suppliers with query: {Query}", query);
            return StatusCode(500, ApiResponse<IEnumerable<object>>.Fail("Internal server error", ex.Message));
        }
    }
}