using ProductCatalog.Api.DTOs;
using ProductCatalog.Api.Interfaces;
using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Services;

public class ProductService : IProductService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 100;
    private const int DefaultLowStockThreshold = 5;

    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<PagedResponseDto<ProductListItemDto>> GetProductsAsync(ProductQueryParametersDto parameters)
    {
        var normalizedParameters = NormalizeParameters(parameters);
        var products = await _productRepository.GetAllAsync();

        var filteredProducts = ApplyFilters(products, normalizedParameters).ToList();
        var totalItems = filteredProducts.Count;
        var totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)normalizedParameters.PageSize);

        var items = filteredProducts
            .Skip((normalizedParameters.Page - 1) * normalizedParameters.PageSize)
            .Take(normalizedParameters.PageSize)
            .Select(MapToListItem)
            .ToList();

        return new PagedResponseDto<ProductListItemDto>
        {
            Items = items,
            Page = normalizedParameters.Page,
            PageSize = normalizedParameters.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }

    public async Task<ProductDetailDto?> GetProductByIdAsync(int idProducto)
    {
        var product = await _productRepository.GetByIdAsync(idProducto);

        return product is null ? null : MapToDetail(product);
    }

    public async Task<IEnumerable<ProductListItemDto>> GetLowStockAsync(int threshold)
    {
        var normalizedThreshold = threshold <= 0 ? DefaultLowStockThreshold : threshold;
        var products = await _productRepository.GetAllAsync();

        return products
            .Where(product => product.Stock > 0 && product.Stock <= normalizedThreshold)
            .Select(MapToListItem)
            .ToList();
    }

    private static ProductQueryParametersDto NormalizeParameters(ProductQueryParametersDto? parameters)
    {
        parameters ??= new ProductQueryParametersDto();

        return new ProductQueryParametersDto
        {
            Search = parameters.Search?.Trim(),
            InStock = parameters.InStock,
            Page = parameters.Page <= 0 ? DefaultPage : parameters.Page,
            PageSize = NormalizePageSize(parameters.PageSize)
        };
    }

    private static int NormalizePageSize(int pageSize)
    {
        if (pageSize <= 0)
        {
            return DefaultPageSize;
        }

        return Math.Min(pageSize, MaxPageSize);
    }

    private static IEnumerable<Product> ApplyFilters(
        IEnumerable<Product> products,
        ProductQueryParametersDto parameters)
    {
        var query = products;

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            query = query.Where(product =>
                product.NombreProducto.Contains(parameters.Search, StringComparison.OrdinalIgnoreCase));
        }

        if (parameters.InStock.HasValue)
        {
            query = parameters.InStock.Value
                ? query.Where(product => product.Stock > 0)
                : query.Where(product => product.Stock == 0);
        }

        return query.OrderBy(product => product.IdProducto);
    }

    private static ProductListItemDto MapToListItem(Product product)
    {
        return new ProductListItemDto
        {
            IdProducto = product.IdProducto,
            NombreProducto = product.NombreProducto,
            Valor = product.Valor,
            Stock = product.Stock
        };
    }

    private static ProductDetailDto MapToDetail(Product product)
    {
        return new ProductDetailDto
        {
            IdProducto = product.IdProducto,
            NombreProducto = product.NombreProducto,
            Descripcion = product.Descripcion,
            Valor = product.Valor,
            Stock = product.Stock,
            EstadoStock = GetEstadoStock(product.Stock)
        };
    }

    private static string GetEstadoStock(int stock)
    {
        if (stock == 0)
        {
            return "Agotado";
        }

        if (stock <= DefaultLowStockThreshold)
        {
            return "Bajo";
        }

        return "Disponible";
    }
}
