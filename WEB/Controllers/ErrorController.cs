using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WEB.Controllers;

public class ErrorController : Controller
{
	[Route("Error/404")]
	public IActionResult NotFound()
	{
		return View("NotFound"); // 404 sayfasını render et
	}

	[Route("Error/500")]
	public IActionResult InternalServerError()
	{
		return View("InternalServerError"); // 500 sayfasını render et
	}

	[Route("Error/{statusCode}")]
	public IActionResult StatusCodeHandler(int statusCode)
	{
		var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
		var exceptionMessage = exceptionHandlerPathFeature?.Error.Message ?? "Bilinmeyen Hata";

		switch (statusCode)
		{
			case 404:
				ViewBag.ErrorMessage = "Aradığınız sayfa bulunamadı.";
				return View("NotFound");
			case 500:
				ViewBag.ErrorMessage = $"Sunucu hatası: {exceptionMessage}";
				return View("InternalServerError");
			default:
				ViewBag.ErrorMessage = $"Hata kodu: {statusCode}";
				return View("Error");
		}
	}
}
