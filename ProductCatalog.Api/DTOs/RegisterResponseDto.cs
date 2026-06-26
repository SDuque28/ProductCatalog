namespace ProductCatalog.Api.DTOs;

public class RegisterResponseDto
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public string Message { get; set; } = string.Empty;
}
