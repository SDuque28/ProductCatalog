namespace ProductCatalog.Api.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public string TokenType { get; set; } = string.Empty;
}
