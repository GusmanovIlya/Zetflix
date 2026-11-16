using System.ComponentModel.DataAnnotations;

namespace Zetflix.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите email.")]
    [EmailAddress(ErrorMessage = "Введите корректный email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от 6 до 100 символов.")]
    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}
