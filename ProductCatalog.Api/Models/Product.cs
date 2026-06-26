namespace ProductCatalog.Api.Models;

public class Product
{
    public int IdProducto { get; set; }

    public string NombreProducto { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public int Stock { get; set; }
}
