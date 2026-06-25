namespace ProductCatalog.Api.DTOs;

public class ProductQueryParametersDto
{
    public string? Search { get; set; }

    public bool? InStock { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}
