using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Api.Options;

public class DemoUserOptions
{
    public const string SectionName = "DemoUser";

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
