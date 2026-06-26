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
    public async Task<ActionResult<LoginResponseDto>> LoginAsync([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
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

    /// <summary>
    /// Registers a user in the TXT-based user store.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <response code="201">Returns the registered user information.</response>
    /// <response code="400">Returned when the request payload is invalid.</response>
    /// <response code="409">Returned when the username or email already exists.</response>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegisterResponseDto>> RegisterAsync([FromBody] RegisterRequestDto request)
    {
        var response = await _authService.RegisterAsync(request);

        return StatusCode(StatusCodes.Status201Created, response);
    }
}
