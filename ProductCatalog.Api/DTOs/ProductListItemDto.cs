namespace ProductCatalog.Api.DTOs;

public class ProductListItemDto
{
    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public int Stock { get; set; }
}
