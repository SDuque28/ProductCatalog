using ProductCatalog.Api.DTOs;

namespace ProductCatalog.Api.Interfaces;

public interface IAuthService
{
    LoginResponseDto? Login(LoginRequestDto request);
}
