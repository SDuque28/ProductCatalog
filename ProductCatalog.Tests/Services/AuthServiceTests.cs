using FluentAssertions;
using Moq;
using Microsoft.Extensions.Options;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Exceptions;
using ProductCatalog.Api.Interfaces;
using ProductCatalog.Api.Options;
using ProductCatalog.Api.Repositories;
using ProductCatalog.Api.Services;
using ProductCatalog.Api.Models;
using Microsoft.AspNetCore.Hosting;

namespace ProductCatalog.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly string _contentRootPath;

    public AuthServiceTests()
    {
        _contentRootPath = Path.Combine(Path.GetTempPath(), $"ProductCatalogAuthTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_contentRootPath, "Data"));
    }

    [Fact]
    public async Task RegisterAsync_WhenRequestIsValid_ShouldCreateUserWithSha256PasswordHash()
    {
        // Arrange
        var repository = CreateRepository();
        var hasher = new Sha256PasswordHasher();
        var service = CreateService(repository, hasher);
        var request = new RegisterRequestDto
        {
            Username = "pablo",
            Email = "pablo@example.com",
            Password = "Password123*",
            ConfirmPassword = "Password123*"
        };

        // Act
        var response = await service.RegisterAsync(request);
        var storedUser = await repository.GetByUsernameAsync("pablo");

        // Assert
        response.Username.Should().Be("pablo");
        response.Email.Should().Be("pablo@example.com");
        response.Message.Should().Be("User registered successfully.");
        storedUser.Should().NotBeNull();
        storedUser!.PasswordHash.Should().Be(hasher.Hash(request.Password));
        storedUser.PasswordHash.Should().NotBe(request.Password);
    }

    [Fact]
    public async Task RegisterAsync_WhenUsernameAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var repository = CreateRepository();
        var service = CreateService(repository, new Sha256PasswordHasher());

        await repository.AddAsync(new User
        {
            Username = "pablo",
            Email = "other@example.com",
            PasswordHash = "HASH",
            CreatedAtUtc = DateTime.UtcNow
        });

        var request = new RegisterRequestDto
        {
            Username = "pablo",
            Email = "pablo@example.com",
            Password = "Password123*",
            ConfirmPassword = "Password123*"
        };

        // Act
        var action = async () => await service.RegisterAsync(request);

        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Username is already registered.");
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var repository = CreateRepository();
        var service = CreateService(repository, new Sha256PasswordHasher());

        await repository.AddAsync(new User
        {
            Username = "someone",
            Email = "pablo@example.com",
            PasswordHash = "HASH",
            CreatedAtUtc = DateTime.UtcNow
        });

        var request = new RegisterRequestDto
        {
            Username = "pablo",
            Email = "pablo@example.com",
            Password = "Password123*",
            ConfirmPassword = "Password123*"
        };

        // Act
        var action = async () => await service.RegisterAsync(request);

        // Assert
        await action.Should().ThrowAsync<ConflictException>()
            .WithMessage("Email is already registered.");
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsMatchRegisteredUser_ShouldReturnToken()
    {
        // Arrange
        var repository = CreateRepository();
        var hasher = new Sha256PasswordHasher();
        var service = CreateService(repository, hasher);

        await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "pablo",
            Email = "pablo@example.com",
            Password = "Password123*",
            ConfirmPassword = "Password123*"
        });

        var request = new LoginRequestDto
        {
            Username = "pablo",
            Password = "Password123*"
        };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrWhiteSpace();
        result.TokenType.Should().Be("Bearer");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordIsInvalid_ShouldReturnNull()
    {
        // Arrange
        var repository = CreateRepository();
        var service = CreateService(repository, new Sha256PasswordHasher());

        await service.RegisterAsync(new RegisterRequestDto
        {
            Username = "pablo",
            Email = "pablo@example.com",
            Password = "Password123*",
            ConfirmPassword = "Password123*"
        });

        var request = new LoginRequestDto
        {
            Username = "pablo",
            Password = "wrong-password"
        };

        // Act
        var result = await service.LoginAsync(request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task EnsureInitializedAsync_WhenUsersFileIsEmpty_ShouldSeedDemoUser()
    {
        // Arrange
        var repository = CreateRepository();
        var service = CreateService(repository, new Sha256PasswordHasher());

        // Act
        await service.EnsureInitializedAsync();
        var seededUser = await repository.GetByUsernameAsync("admin");

        // Assert
        seededUser.Should().NotBeNull();
        seededUser!.Email.Should().Be("admin@example.com");
    }

    public void Dispose()
    {
        if (Directory.Exists(_contentRootPath))
        {
            Directory.Delete(_contentRootPath, recursive: true);
        }
    }

    private AuthService CreateService(IUserRepository repository, IPasswordHasher passwordHasher)
    {
        var demoUserOptions = Options.Create(new DemoUserOptions
        {
            Username = "admin",
            Email = "admin@example.com",
            Password = "Admin123*"
        });

        var jwtOptions = Options.Create(new JwtOptions
        {
            Issuer = "ProductCatalog.Api",
            Audience = "ProductCatalog.Client",
            SecretKey = "AssessmentOnlySecretKey-ReplaceInProduction-2026",
            ExpirationMinutes = 60
        });

        return new AuthService(demoUserOptions, jwtOptions, repository, passwordHasher);
    }

    private TxtUserRepository CreateRepository()
    {
        var environmentMock = new Mock<IWebHostEnvironment>();
        environmentMock.SetupGet(environment => environment.ContentRootPath).Returns(_contentRootPath);

        return new TxtUserRepository(environmentMock.Object);
    }
}
