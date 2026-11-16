using System.ComponentModel.DataAnnotations;

namespace Zetflix.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите имя пользователя.")]
    [StringLength(40, MinimumLength = 2, ErrorMessage = "Имя пользователя должно быть от 2 до 40 символов.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите email.")]
    [EmailAddress(ErrorMessage = "Введите корректный email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль должен быть от 8 до 100 символов.")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "Пароль должен содержать строчную букву, заглавную букву и цифру.")]
    public string Password { get; set; } = string.Empty;
}
