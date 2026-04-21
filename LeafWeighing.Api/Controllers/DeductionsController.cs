using Microsoft.AspNetCore.Mvc;
using LeafWeighing.Application.DTOs.Common;
using LeafWeighing.Application.DTOs.Deduction;
using LeafWeighing.Application.Interfaces.Services;

namespace LeafWeighing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeductionsController : ControllerBase
{
    private readonly IDeductionService _deductionService;
    private readonly ILogger<DeductionsController> _logger;

    public DeductionsController(IDeductionService deductionService, ILogger<DeductionsController> logger)
    {
        _deductionService = deductionService;
        _logger = logger;
    }

    [HttpGet("summary/{regNo}")]
    public async Task<ActionResult<ApiResponse<DeductionSummaryDto>>> GetDeductionSummary(
        int regNo,
        [FromHeader(Name = "leaf-type")] string leafType)
    {
        try
        {
            if (string.IsNullOrEmpty(leafType))
            {
                return BadRequest(ApiResponse<DeductionSummaryDto>.Fail("Leaf type is required"));
            }

            var summary = await _deductionService.GetDeductionSummaryAsync(regNo, leafType);
            return Ok(ApiResponse<DeductionSummaryDto>.Ok(summary));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting deduction summary for RegNo: {RegNo}, LeafType: {LeafType}", regNo, leafType);
            return StatusCode(500, ApiResponse<DeductionSummaryDto>.Fail("Internal server error", ex.Message));
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<object>>> SaveDeduction([FromBody] SaveDeductionRequestDto request)
    {
        try
        {
            if (request.RegNo == 0 || string.IsNullOrEmpty(request.SupplierName))
            {
                return BadRequest(ApiResponse<object>.Fail("Missing required fields: regNo, supplierName"));
            }

            var result = await _deductionService.SaveDeductionAsync(request);
            return StatusCode(201, ApiResponse<object>.Ok(result, "Deduction saved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving deduction for RegNo: {RegNo}", request.RegNo);
            return StatusCode(500, ApiResponse<object>.Fail("Internal server error", ex.Message));
        }
    }

    [HttpGet("today/{regNo}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TransactionDto>>>> GetTodayTransactions(int regNo)
    {
        try
        {
            var transactions = await _deductionService.GetTodayTransactionsAsync(regNo);
            return Ok(ApiResponse<IEnumerable<TransactionDto>>.Ok(transactions));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's transactions for RegNo: {RegNo}", regNo);
            return StatusCode(500, ApiResponse<IEnumerable<TransactionDto>>.Fail("Internal server error", ex.Message));
        }
    }
}