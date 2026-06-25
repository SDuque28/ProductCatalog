using Microsoft.AspNetCore.Hosting;
using FluentAssertions;
using Moq;
using ProductCatalog.Api.Repositories;

namespace ProductCatalog.Tests.Repositories;

public class TxtProductRepositoryTests : IDisposable
{
    private readonly string _contentRootPath;

    public TxtProductRepositoryTests()
    {
        _contentRootPath = Path.Combine(Path.GetTempPath(), $"ProductCatalogTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path.Combine(_contentRootPath, "Data"));
    }

    [Fact]
    public async Task GetAllAsync_WhenFileContainsMalformedRows_ShouldSkipInvalidRows()
    {
        // Arrange
        var content = """
            IdProducto|NombreProducto|Descripcion|Valor|Stock
            1|Teclado mecanico|Teclado RGB switch azul|150000|12
            abc|Producto invalido|Id invalido|50000|3
            2|Mouse|Valor invalido|abc|1
            3||Producto sin nombre|120000|5
            4|Monitor 24|Monitor IPS Full HD|620000|4
            1|Teclado duplicado|Debe ignorarse|180000|8
            """;

        await WriteProductsFileAsync(content);
        var repository = CreateRepository();

        // Act
        var result = (await repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(product => product.IdProducto).Should().Equal(1, 4);
    }

    [Fact]
    public async Task GetAllAsync_WhenFileContainsEmptyLines_ShouldIgnoreThem()
    {
        // Arrange
        var content = """
            IdProducto|NombreProducto|Descripcion|Valor|Stock

            1|Teclado mecanico|Teclado RGB switch azul|150000|12

            2|Mouse inalambrico|Mouse ergonomico 2.4GHz|85000|0

            """;

        await WriteProductsFileAsync(content);
        var repository = CreateRepository();

        // Act
        var result = (await repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result.Select(product => product.IdProducto).Should().Equal(1, 2);
    }

    [Fact]
    public async Task GetAllAsync_WhenFileContainsValidRows_ShouldReturnProducts()
    {
        // Arrange
        var content = """
            IdProducto|NombreProducto|Descripcion|Valor|Stock
            1|Teclado mecanico|Teclado RGB switch azul|150000|12
            2|Mouse inalambrico|Mouse ergonomico 2.4GHz|85000|0
            3|Monitor 24|Monitor IPS Full HD|620000|4
            """;

        await WriteProductsFileAsync(content);
        var repository = CreateRepository();

        // Act
        var result = (await repository.GetAllAsync()).ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].NombreProducto.Should().Be("Teclado mecanico");
        result[1].Valor.Should().Be(85000m);
        result[2].Stock.Should().Be(4);
    }

    public void Dispose()
    {
        if (Directory.Exists(_contentRootPath))
        {
            Directory.Delete(_contentRootPath, recursive: true);
        }
    }

    private TxtProductRepository CreateRepository()
    {
        var environmentMock = new Mock<IWebHostEnvironment>();
        environmentMock.SetupGet(environment => environment.ContentRootPath).Returns(_contentRootPath);

        return new TxtProductRepository(environmentMock.Object);
    }

    private Task WriteProductsFileAsync(string content)
    {
        var filePath = Path.Combine(_contentRootPath, "Data", "products.txt");

        return File.WriteAllTextAsync(filePath, content.ReplaceLineEndings(Environment.NewLine));
    }
}
