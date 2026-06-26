using ProductCatalog.Api.DTOs;

namespace ProductCatalog.Api.Interfaces;

public interface IAuthService
{
    Task EnsureInitializedAsync();

    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);

    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
}
