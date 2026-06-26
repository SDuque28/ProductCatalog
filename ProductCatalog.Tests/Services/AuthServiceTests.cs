using FluentAssertions;
using Microsoft.Extensions.Options;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Options;
using ProductCatalog.Api.Services;

namespace ProductCatalog.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public void Login_WhenCredentialsAreValid_ShouldReturnToken()
    {
        // Arrange
        var service = CreateService();
        var request = new LoginRequestDto
        {
            Username = "admin",
            Password = "Admin123*"
        };

        // Act
        var result = service.Login(request);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.TokenType.Should().Be("Bearer");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public void Login_WhenCredentialsAreInvalid_ShouldReturnNull()
    {
        // Arrange
        var service = CreateService();
        var request = new LoginRequestDto
        {
            Username = "admin",
            Password = "wrong-password"
        };

        // Act
        var result = service.Login(request);

        // Assert
        result.Should().BeNull();
    }

    private static AuthService CreateService()
    {
        var demoUserOptions = Options.Create(new DemoUserOptions
        {
            Username = "admin",
            Password = "Admin123*"
        });

        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "ProductCatalog.Api",
            Audience = "ProductCatalog.Client",
            SecretKey = "AssessmentOnlySecretKey-ReplaceInProduction-2026",
            ExpirationMinutes = 60
        });

        return new AuthService(demoUserOptions, jwtOptions);
    }
}
