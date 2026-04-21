using Microsoft.AspNetCore.Mvc;
using LeafWeighing.Application.DTOs.Common;
using LeafWeighing.Application.DTOs.LeafCount;
using LeafWeighing.Application.Interfaces.Services;

namespace LeafWeighing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeafCountController : ControllerBase
{
    private readonly ILeafCountService _leafCountService;
    private readonly ILogger<LeafCountController> _logger;

    public LeafCountController(ILeafCountService leafCountService, ILogger<LeafCountController> logger)
    {
        _leafCountService = leafCountService;
        _logger = logger;
    }

    [HttpGet("routes")]
    public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetRoutes()
    {
        try
        {
            var routes = await _leafCountService.GetDistinctRoutesAsync();
            return Ok(ApiResponse<IEnumerable<string>>.Ok(routes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting distinct routes");
            return StatusCode(500, ApiResponse<IEnumerable<string>>.Fail("Failed to fetch routes", ex.Message));
        }
    }

    [HttpGet("routes/{routeName}/total-weight")]
    public async Task<ActionResult<ApiResponse<RouteTotalWeightDto>>> GetRouteTotalWeight(
        string routeName,
        [FromQuery] int date,
        [FromQuery] string month)
    {
        try
        {
            if (string.IsNullOrEmpty(routeName))
            {
                return BadRequest(ApiResponse<RouteTotalWeightDto>.Fail("Route name is required"));
            }

            if (date == 0 || string.IsNullOrEmpty(month))
            {
                return BadRequest(ApiResponse<RouteTotalWeightDto>.Fail("Date and month are required"));
            }

            var result = await _leafCountService.GetRouteTotalWeightAsync(routeName, date, month);
            return Ok(ApiResponse<RouteTotalWeightDto>.Ok(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting route total weight for Route: {RouteName}, Date: {Date}, Month: {Month}", routeName, date, month);
            return StatusCode(500, ApiResponse<RouteTotalWeightDto>.Fail("Failed to fetch route total weight", ex.Message));
        }
    }

    [HttpPost("save")]
    public async Task<ActionResult<ApiResponse<object>>> SaveLeafCount([FromBody] LeafCountRequestDto request)
    {
        try
        {
            if (request.Date == 0 || string.IsNullOrEmpty(request.Month) || string.IsNullOrEmpty(request.Route))
            {
                return BadRequest(ApiResponse<object>.Fail("Missing required fields: date, month, route"));
            }

            if (request.BestLeaf == 0 && request.BellowBest == 0 && request.Poor == 0)
            {
                return BadRequest(ApiResponse<object>.Fail("At least one leaf count value is required"));
            }

            var result = await _leafCountService.SaveLeafCountAsync(request);
            return StatusCode(201, ApiResponse<object>.Ok(result, "Leaf count saved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving leaf count for Route: {Route}", request.Route);
            return StatusCode(500, ApiResponse<object>.Fail("Failed to save leaf count", ex.Message));
        }
    }

    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<LeafCountResponseDto>>>> GetLeafCountHistory(
        [FromQuery] string? month,
        [FromQuery] string? route,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        try
        {
            var history = await _leafCountService.GetLeafCountHistoryAsync(month, route, startDate, endDate);
            return Ok(ApiResponse<IEnumerable<LeafCountResponseDto>>.Ok(history, $"Found {history.Count()} records"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leaf count history");
            return StatusCode(500, ApiResponse<IEnumerable<LeafCountResponseDto>>.Fail("Failed to fetch leaf count history", ex.Message));
        }
    }
}