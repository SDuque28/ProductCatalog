namespace ProductCatalog.Api.DTOs;

/// <summary>
/// Represents a standardized error payload returned by the API.
/// </summary>
public class ErrorResponseDto
{
    /// <summary>
    /// Gets or sets the HTTP status code associated with the error.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the user-facing error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets optional error details that are safe to expose.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp for when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }
}
