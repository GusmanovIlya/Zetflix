using Microsoft.AspNetCore.Mvc;

namespace Zetflix.Controllers;

public class ErrorController : Controller
{
    [Route("Error/404")]
    public IActionResult NotFoundPage()
    {
        Response.StatusCode = 404;
        return View("NotFound");
    }

    [Route("Error/403")]
    public IActionResult ForbiddenPage()
    {
        Response.StatusCode = 403;
        return View("Forbidden");
    }

    [Route("Error/500")]
    public IActionResult ServerErrorPage()
    {
        Response.StatusCode = 500;
        return View("ServerError");
    }

    [Route("Error/{statusCode:int}")]
    public IActionResult StatusCodePage(int statusCode)
    {
        Response.StatusCode = statusCode;

        return statusCode switch
        {
            403 => View("Forbidden"),
            404 => View("NotFound"),
            500 => View("ServerError"),
            _ => View("ServerError")
        };
    }
}
