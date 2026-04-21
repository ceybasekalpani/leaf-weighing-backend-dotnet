using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using LeafWeighing.Application.DTOs.Auth;
using LeafWeighing.Application.Interfaces.Repositories;
using LeafWeighing.Application.Interfaces.Services;
using LeafWeighing.Domain.Entities;

namespace LeafWeighing.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserSetupRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IUserSetupRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress)
    {
        _logger.LogInformation("Login attempt for username: {Username}", request.Username);

        var user = await _userRepository.GetUserByCredentialsAsync(request.Username, request.Password);

        if (user == null)
        {
            _logger.LogWarning("Login failed for username: {Username}", request.Username);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        _logger.LogInformation("Login successful for username: {Username}", request.Username);

        var token = GenerateJwtToken(user);

        return new LoginResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Ind = user.Ind,
                FullName = user.FullName,
                UserName = user.UserName,
                Admin = user.Admin,
                AdminLevel = user.AdminLevel,
                Active = user.Active,
                TempWorker = user.TempWorker,
                Permissions = new UserPermissionsDto
                {
                    Confirm = user.BlConfirm,
                    Report = user.BlReport,
                    Transfer = user.BlTransfer,
                    LeafEditDel = user.BlLeafEditDel,
                    BackDays = user.BackDays
                }
            }
        };
    }

    private string GenerateJwtToken(UserSetup user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "leaf-weighing-secret-key-2024-for-development-only"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Ind.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.GivenName, user.FullName ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "leaf-weighing-secret-key-2024-for-development-only");

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
}