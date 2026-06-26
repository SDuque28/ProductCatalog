using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Interfaces;

namespace ProductCatalog.Tests.Controllers;

public class ProductsControllerTests
{
    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnOk()
    {
        // Arrange
        var productServiceMock = new Mock<IProductService>();
        productServiceMock
            .Setup(service => service.GetProductByIdAsync(1))
            .ReturnsAsync(new ProductDetailDto
            {
                IdProducto = 1,
                NombreProducto = "Teclado mecanico",
                Descripcion = "Teclado RGB switch azul",
                Valor = 150000m,
                Stock = 12,
                EstadoStock = "Disponible"
            });

        await using var factory = new TestProductApiFactory(productServiceMock.Object);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        await AuthenticateAsync(client);

        // Act
        var response = await client.GetAsync("/api/products/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        var productServiceMock = new Mock<IProductService>();
        productServiceMock
            .Setup(service => service.GetProductByIdAsync(999))
            .ReturnsAsync((ProductDetailDto?)null);

        await using var factory = new TestProductApiFactory(productServiceMock.Object);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        await AuthenticateAsync(client);

        // Act
        var response = await client.GetAsync("/api/products/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProductsAsync_WhenRequestIsValid_ShouldReturnOk()
    {
        // Arrange
        var pagedResponse = new PagedResponseDto<ProductListItemDto>
        {
            Page = 1,
            PageSize = 10,
            TotalItems = 1,
            Items =
            [
                new ProductListItemDto
                {
                    IdProducto = 1,
                    NombreProducto = "Teclado mecanico",
                    Valor = 150000m,
                    Stock = 12
                }
            ]
        };

        var productServiceMock = new Mock<IProductService>();
        productServiceMock
            .Setup(service => service.GetProductsAsync(It.IsAny<ProductQueryParametersDto>()))
            .ReturnsAsync(pagedResponse);

        await using var factory = new TestProductApiFactory(productServiceMock.Object);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        await AuthenticateAsync(client);

        // Act
        var response = await client.GetAsync("/api/products?name=teclado&inStock=true&page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetProductsAsync_WhenRequestIsUnauthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        var productServiceMock = new Mock<IProductService>();

        await using var factory = new TestProductApiFactory(productServiceMock.Object);
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static async Task AuthenticateAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Username = "admin",
            Password = "Admin123*"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            loginResponse.TokenType,
            loginResponse.Token);
    }
}
