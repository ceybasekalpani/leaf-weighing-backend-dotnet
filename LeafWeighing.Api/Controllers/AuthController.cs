using Microsoft.AspNetCore.Mvc;
using LeafWeighing.Application.DTOs.Auth;
using LeafWeighing.Application.DTOs.Common;
using LeafWeighing.Application.Interfaces.Services;

namespace LeafWeighing.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.LoginAsync(request, ipAddress);

            return Ok(ApiResponse<LoginResponseDto>.Ok(result, "Login successful"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(ApiResponse<LoginResponseDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return StatusCode(500, ApiResponse<LoginResponseDto>.Fail("Internal server error", ex.Message));
        }
    }

    [HttpPost("verify-token")]
    public ActionResult<ApiResponse<object>> VerifyToken([FromHeader(Name = "Authorization")] string authorization)
    {
        try
        {
            var token = authorization?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(ApiResponse<object>.Fail("No token provided"));
            }

            var isValid = _authService.ValidateToken(token);

            if (!isValid)
            {
                return Unauthorized(ApiResponse<object>.Fail("Invalid token"));
            }

            return Ok(ApiResponse<object>.Ok(new { }, "Token is valid"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token verification");
            return StatusCode(500, ApiResponse<object>.Fail("Token verification failed", ex.Message));
        }
    }
}