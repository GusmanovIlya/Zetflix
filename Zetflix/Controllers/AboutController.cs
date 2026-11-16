using Microsoft.AspNetCore.Mvc;

namespace Zetflix.Controllers;

public class AboutController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
