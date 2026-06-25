namespace ProductCatalog.Api.DTOs;

public class ProductQueryParameters
{
    public string? Name { get; set; }

    public bool? InStock { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
