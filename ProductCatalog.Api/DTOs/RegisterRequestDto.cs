using System.ComponentModel.DataAnnotations;

namespace ProductCatalog.Api.DTOs;

public class RegisterRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Password and confirmPassword must match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
