using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Interfaces;
using ProductCatalog.Api.Options;

namespace ProductCatalog.Api.Services;

public class AuthService : IAuthService
{
    private readonly DemoUserOptions _demoUserOptions;
    private readonly JwtOptions _jwtOptions;

    public AuthService(IOptions<DemoUserOptions> demoUserOptions, IOptions<JwtOptions> jwtOptions)
    {
        _demoUserOptions = demoUserOptions.Value;
        _jwtOptions = jwtOptions.Value;
    }

    public LoginResponseDto? Login(LoginRequestDto request)
    {
        if (!IsValidCredential(request))
        {
            return null;
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, _demoUserOptions.Username),
            new(JwtRegisteredClaimNames.UniqueName, _demoUserOptions.Username),
            new(ClaimTypes.Name, _demoUserOptions.Username)
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

    private bool IsValidCredential(LoginRequestDto request)
    {
        return string.Equals(request.Username, _demoUserOptions.Username, StringComparison.Ordinal)
            && string.Equals(request.Password, _demoUserOptions.Password, StringComparison.Ordinal);
    }
}
