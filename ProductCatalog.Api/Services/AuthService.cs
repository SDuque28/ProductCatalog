using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Exceptions;
using ProductCatalog.Api.Interfaces;
using ProductCatalog.Api.Models;
using ProductCatalog.Api.Options;

namespace ProductCatalog.Api.Services;

public class AuthService : IAuthService
{
    private readonly JwtOptions _jwtOptions;
    private readonly DemoUserOptions _demoUserOptions;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IOptions<DemoUserOptions> demoUserOptions,
        IOptions<JwtOptions> jwtOptions,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _jwtOptions = jwtOptions.Value;
        _demoUserOptions = demoUserOptions.Value;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task EnsureInitializedAsync()
    {
        await EnsureDemoUserAsync();
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request)
    {
        await EnsureDemoUserAsync();

        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        return CreateLoginResponse(user.Username);
    }

    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        await EnsureDemoUserAsync();
        ValidateRegisterRequest(request);

        var normalizedUsername = request.Username.Trim();
        var normalizedEmail = request.Email.Trim();

        if (await _userRepository.GetByUsernameAsync(normalizedUsername) is not null)
        {
            throw new ConflictException("Username is already registered.");
        }

        if (await _userRepository.GetByEmailAsync(normalizedEmail) is not null)
        {
            throw new ConflictException("Email is already registered.");
        }

        var user = new User
        {
            Username = normalizedUsername,
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);

        return new RegisterResponseDto
        {
            Username = user.Username,
            Email = user.Email,
            CreatedAtUtc = user.CreatedAtUtc,
            Message = "User registered successfully."
        };
    }

    private LoginResponseDto CreateLoginResponse(string username)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(ClaimTypes.Name, username)
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            TokenType = JwtBearerDefaults.AuthenticationScheme
        };
    }

    private static void ValidateRegisterRequest(RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            throw new BadRequestException("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new BadRequestException("Email is required.");
        }

        if (!new EmailAddressAttribute().IsValid(request.Email.Trim()))
        {
            throw new BadRequestException("Email must be a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new BadRequestException("Password is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            throw new BadRequestException("ConfirmPassword is required.");
        }

        if (request.Password.Length < 8)
        {
            throw new BadRequestException("Password must be at least 8 characters long.");
        }

        if (!string.Equals(request.Password, request.ConfirmPassword, StringComparison.Ordinal))
        {
            throw new BadRequestException("Password and confirmPassword must match.");
        }
    }

    private async Task EnsureDemoUserAsync()
    {
        var existingUserByUsername = await _userRepository.GetByUsernameAsync(_demoUserOptions.Username);
        if (existingUserByUsername is not null)
        {
            return;
        }

        var existingUserByEmail = await _userRepository.GetByEmailAsync(_demoUserOptions.Email);
        if (existingUserByEmail is not null)
        {
            return;
        }

        var demoUser = new User
        {
            Username = _demoUserOptions.Username,
            Email = _demoUserOptions.Email,
            PasswordHash = _passwordHasher.Hash(_demoUserOptions.Password),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _userRepository.AddAsync(demoUser);
    }
}
