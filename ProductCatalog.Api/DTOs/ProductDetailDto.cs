namespace ProductCatalog.Api.DTOs;

public class ProductDetailDto
{
    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public int Stock { get; set; }

    public string EstadoStock { get; set; } = string.Empty;
}
