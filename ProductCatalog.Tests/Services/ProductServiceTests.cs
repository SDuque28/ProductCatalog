using FluentAssertions;
using Moq;
using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Interfaces;
using ProductCatalog.Api.Models;
using ProductCatalog.Api.Services;

namespace ProductCatalog.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock = new();

    [Fact]
    public async Task GetProductsAsync_WhenNoFilters_ShouldReturnAllProducts()
    {
        // Arrange
        var products = CreateProducts();
        _repositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(products);

        var service = CreateService();
        var parameters = new ProductQueryParametersDto();

        // Act
        var result = await service.GetProductsAsync(parameters);

        // Assert
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.Total.Should().Be(products.Count);
        result.Items.Should().HaveCount(products.Count);
        result.Items.Select(product => product.IdProducto).Should().Equal(1, 2, 3, 4);
    }

    [Fact]
    public async Task GetProductsAsync_WhenSearchingByName_ShouldReturnMatchingProducts()
    {
        // Arrange
        var products = CreateProducts();
        _repositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(products);

        var service = CreateService();
        var parameters = new ProductQueryParametersDto
        {
            Search = "MOUSE"
        };

        // Act
        var result = await service.GetProductsAsync(parameters);

        // Assert
        result.Total.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Select(product => product.NombreProducto)
            .Should()
            .Equal("Mouse Inalambrico", "Gaming Mouse Pad");
    }

    [Fact]
    public async Task GetProductsAsync_WhenFilteringByStock_ShouldReturnOnlyAvailableProducts()
    {
        // Arrange
        var products = CreateProducts();
        _repositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(products);

        var service = CreateService();
        var parameters = new ProductQueryParametersDto
        {
            InStock = true
        };

        // Act
        var result = await service.GetProductsAsync(parameters);

        // Assert
        result.Total.Should().Be(3);
        result.Items.Should().OnlyContain(product => product.Stock > 0);
        result.Items.Select(product => product.IdProducto).Should().Equal(1, 3, 4);
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductExists_ShouldReturnProduct()
    {
        // Arrange
        var product = new Product
        {
            IdProducto = 3,
            NombreProducto = "Monitor 24",
            Descripcion = "Monitor IPS Full HD",
            Valor = 620000m,
            Stock = 4
        };

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(product.IdProducto))
            .ReturnsAsync(product);

        var service = CreateService();

        // Act
        var result = await service.GetProductByIdAsync(product.IdProducto);

        // Assert
        result.Should().NotBeNull();
        result!.IdProducto.Should().Be(product.IdProducto);
        result.NombreProducto.Should().Be(product.NombreProducto);
        result.Descripcion.Should().Be(product.Descripcion);
        result.Valor.Should().Be(product.Valor);
        result.Stock.Should().Be(product.Stock);
        result.EstadoStock.Should().Be("Bajo");
    }

    [Fact]
    public async Task GetProductByIdAsync_WhenProductDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        var service = CreateService();

        // Act
        var result = await service.GetProductByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetProductsAsync_WhenPaginationIsApplied_ShouldReturnCorrectPage()
    {
        // Arrange
        var products = CreateProducts(count: 12);
        _repositoryMock
            .Setup(repository => repository.GetAllAsync())
            .ReturnsAsync(products);

        var service = CreateService();
        var parameters = new ProductQueryParametersDto
        {
            Page = 2,
            PageSize = 5
        };

        // Act
        var result = await service.GetProductsAsync(parameters);

        // Assert
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.Total.Should().Be(12);
        result.Items.Should().HaveCount(5);
        result.Items.Select(product => product.IdProducto).Should().Equal(6, 7, 8, 9, 10);
    }

    private ProductService CreateService()
    {
        return new ProductService(_repositoryMock.Object);
    }

    private static List<Product> CreateProducts(int count = 4)
    {
        if (count == 4)
        {
            return
            [
                new Product
                {
                    IdProducto = 1,
                    NombreProducto = "Teclado Mecanico",
                    Descripcion = "Teclado RGB switch azul",
                    Valor = 150000m,
                    Stock = 12
                },
                new Product
                {
                    IdProducto = 2,
                    NombreProducto = "Mouse Inalambrico",
                    Descripcion = "Mouse ergonomico 2.4GHz",
                    Valor = 85000m,
                    Stock = 0
                },
                new Product
                {
                    IdProducto = 3,
                    NombreProducto = "Gaming Mouse Pad",
                    Descripcion = "Mouse pad extendido",
                    Valor = 45000m,
                    Stock = 4
                },
                new Product
                {
                    IdProducto = 4,
                    NombreProducto = "Monitor 24",
                    Descripcion = "Monitor IPS Full HD",
                    Valor = 620000m,
                    Stock = 6
                }
            ];
        }

        return Enumerable.Range(1, count)
            .Select(index => new Product
            {
                IdProducto = index,
                NombreProducto = $"Producto {index}",
                Descripcion = $"Descripcion {index}",
                Valor = 1000m * index,
                Stock = index
            })
            .ToList();
    }
}
