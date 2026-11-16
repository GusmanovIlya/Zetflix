using System.ComponentModel.DataAnnotations;
using Zetflix.Models;

namespace Zetflix.ViewModels;

public class ContactViewModel
{
    public SiteSettings Settings { get; set; } = SiteSettings.Default;

    [Required(ErrorMessage = "Введите имя.")]
    [StringLength(80, MinimumLength = 2, ErrorMessage = "Имя должно быть от 2 до 80 символов.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите email.")]
    [EmailAddress(ErrorMessage = "Введите корректный email.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите тему.")]
    [StringLength(120, MinimumLength = 3, ErrorMessage = "Тема должна быть от 3 до 120 символов.")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите сообщение.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Сообщение должно быть от 10 до 2000 символов.")]
    public string Message { get; set; } = string.Empty;

    public bool? Sent { get; set; }
}
