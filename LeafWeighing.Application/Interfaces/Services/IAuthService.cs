using LeafWeighing.Application.DTOs.Auth;

namespace LeafWeighing.Application.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress);
    bool ValidateToken(string token);
}