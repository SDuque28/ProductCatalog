using ProductCatalog.Api.Models;

namespace ProductCatalog.Api.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();

    Task<Product?> GetByIdAsync(int idProducto);
}
