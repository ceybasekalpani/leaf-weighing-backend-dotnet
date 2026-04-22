using Microsoft.AspNetCore.Mvc;
using LeafWeighing.Application.DTOs.Collection;
using LeafWeighing.Application.DTOs.Common;
using LeafWeighing.Application.Interfaces.Services;

namespace LeafWeighing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _collectionService;
    private readonly ILogger<CollectionsController> _logger;

    public CollectionsController(ICollectionService collectionService, ILogger<CollectionsController> logger)
    {
        _collectionService = collectionService;
        _logger = logger;
    }

    [HttpGet("today")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CollectionSummaryDto>>>> GetTodayCollections()
    {
        try
        {
            var collections = await _collectionService.GetTodayCollectionsAsync();
            return Ok(ApiResponse<IEnumerable<CollectionSummaryDto>>.Ok(collections));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting today's collections");
            return StatusCode(500, ApiResponse<IEnumerable<CollectionSummaryDto>>.Fail("Internal server error", ex.Message));
        }
    }

    [HttpGet("date/{date}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CollectionSummaryDto>>>> GetCollectionsByDate(string date)
    {
        try
        {
            if (!DateTime.TryParse(date, out var parsedDate))
            {
                return BadRequest(ApiResponse<IEnumerable<CollectionSummaryDto>>.Fail("Invalid date format. Use yyyy-MM-dd"));
            }

            var collections = await _collectionService.GetCollectionsByDateAsync(parsedDate);
            return Ok(ApiResponse<IEnumerable<CollectionSummaryDto>>.Ok(collections));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collections for date: {Date}", date);
            return StatusCode(500, ApiResponse<IEnumerable<CollectionSummaryDto>>.Fail("Internal server error", ex.Message));
        }
    }

    [HttpGet("{regNo}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CollectionDetailDto>>>> GetCollectionDetails(int regNo)
    {
        try
        {
            var details = await _collectionService.GetCollectionDetailsAsync(regNo);
            return Ok(ApiResponse<IEnumerable<CollectionDetailDto>>.Ok(details));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collection details for RegNo: {RegNo}", regNo);
            return StatusCode(500, ApiResponse<IEnumerable<CollectionDetailDto>>.Fail("Internal server error", ex.Message));
        }
    }
}
