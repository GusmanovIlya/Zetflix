using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Zetflix.ViewModels;

public class ReviewCreateViewModel
{
    [Required(ErrorMessage = "Введите оценку.")]
    [Range(1, 5, ErrorMessage = "Оценка должна быть от 1 до 5.")]
    public int Rating { get; set; }

    [Required(ErrorMessage = "Введите комментарий.")]
    [StringLength(1000, MinimumLength = 3, ErrorMessage = "Комментарий должен быть от 3 до 1000 символов.")]
    public string Comment { get; set; } = string.Empty;

    public int? MovieId { get; set; }
    public int? ShowId { get; set; }

    public List<SelectListItem> Movies { get; set; } = new();
    public List<SelectListItem> Shows { get; set; } = new();
}
