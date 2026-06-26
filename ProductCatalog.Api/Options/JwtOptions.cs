using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Api.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MinLength(32)]
    public string SecretKey { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ExpirationMinutes { get; set; }
}
