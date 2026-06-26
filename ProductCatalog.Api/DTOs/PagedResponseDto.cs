using System.Text.Json.Serialization;

namespace ProductCatalog.Api.DTOs;

public class PagedResponseDto<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int Total => TotalItems;

    [JsonIgnore]
    public int TotalItems { get; set; }

    [JsonIgnore]
    public int TotalPages { get; set; }
}
