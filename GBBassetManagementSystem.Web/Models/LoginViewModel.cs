using System.ComponentModel.DataAnnotations;

namespace GBBassetManagementSystem.Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "UsernameOrEmailRequired")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "PasswordRequired")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}