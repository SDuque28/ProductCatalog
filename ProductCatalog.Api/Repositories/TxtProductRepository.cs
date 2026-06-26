using System.Globalization;
using ProductCatalog.Api.Interfaces;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Repositories;

public class TxtProductRepository : IProductRepository
{
    private const char Separator = '|';
    private const int ExpectedColumnCount = 5;

    private readonly string _filePath;

    public TxtProductRepository(IWebHostEnvironment environment)
    {
        _filePath = Path.Combine(environment.ContentRootPath, "Data", "products.txt");
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await LoadProductsAsync();
    }

    public async Task<Product?> GetByIdAsync(int idProducto)
    {
        var products = await LoadProductsAsync();

        return products.FirstOrDefault(product => product.IdProducto == idProducto);
    }

    private async Task<IReadOnlyList<Product>> LoadProductsAsync()
    {
        var lines = await File.ReadAllLinesAsync(_filePath);
        var products = new List<Product>();
        var existingIds = new HashSet<int>();

        foreach (var line in lines.Skip(1))
        {
            if (!TryParseProduct(line, existingIds, out var product))
            {
                continue;
            }

            products.Add(product);
        }

        return products;
    }

    private static bool TryParseProduct(string? line, ISet<int> existingIds, out Product product)
    {
        product = null!;

        if (string.IsNullOrWhiteSpace(line))
        {
            return false;
        }

        var columns = line.Split(Separator);
        if (columns.Length < ExpectedColumnCount)
        {
            return false;
        }

        if (!TryParseInt(columns[0], out var idProducto) || existingIds.Contains(idProducto))
        {
            return false;
        }

        var nombreProducto = columns[1].Trim();
        if (string.IsNullOrWhiteSpace(nombreProducto))
        {
            return false;
        }

        if (!TryParseDecimal(columns[3], out var valor) || !TryParseInt(columns[4], out var stock))
        {
            return false;
        }

        existingIds.Add(idProducto);

        product = new Product
        {
            IdProducto = idProducto,
            NombreProducto = nombreProducto,
            Descripcion = columns[2].Trim(),
            Valor = valor,
            Stock = stock
        };

        return true;
    }

    private static bool TryParseInt(string value, out int result)
    {
        return int.TryParse(
            value.Trim(),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out result);
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        return decimal.TryParse(
            value.Trim(),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out result);
    }
}
