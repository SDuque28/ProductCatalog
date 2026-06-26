using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Interfaces;

namespace ProductCatalog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticates the demo user and returns a JWT access token.
    /// </summary>
    /// <param name="request">The login credentials.</param>
    /// <response code="200">Returns the generated JWT token.</response>
    /// <response code="401">Returned when the credentials are invalid.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public ActionResult<LoginResponseDto> Login([FromBody] LoginRequestDto request)
    {
        var response = _authService.Login(request);
        if (response is null)
        {
            return Unauthorized(new ErrorResponseDto
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Message = "Invalid username or password.",
                Timestamp = DateTime.UtcNow
            });
        }

        return Ok(response);
    }
}
