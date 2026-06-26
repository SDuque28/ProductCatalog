using ProductCatalog.Api.DTOs;

namespace ProductCatalog.Api.Interfaces;

public interface IProductService
{
    Task<PagedResponseDto<ProductListItemDto>> GetProductsAsync(ProductQueryParametersDto parameters);

    Task<ProductDetailDto?> GetProductByIdAsync(int idProducto);

    Task<IEnumerable<ProductListItemDto>> GetLowStockAsync(int threshold);
}
