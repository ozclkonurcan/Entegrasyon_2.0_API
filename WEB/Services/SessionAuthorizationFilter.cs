using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using WEB.Models.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WEB.Services;


//public class SessionAuthorizationFilter : IActionFilter
//{
//	public void OnActionExecuting(ActionExecutingContext context)
//	{
//		// Login sayfasına erişimi kontrol etme
//		if (context.ActionDescriptor.RouteValues["controller"] == "Auth" &&
//			context.ActionDescriptor.RouteValues["action"] == "Login")
//		{
//			return;
//		}

//		// Setup action'ını filtre dışında tutma
//		if (context.ActionDescriptor.RouteValues["controller"] == "Auth" &&
//			context.ActionDescriptor.RouteValues["action"] == "Setup")
//		{
//			return;
//		}

//		// sqlBaglatinAyarlari action methodunu filtre dışında tutma
//		if (context.ActionDescriptor.RouteValues["controller"] == "Settings" &&
//			context.ActionDescriptor.RouteValues["action"] == "sqlBaglatinAyarlari")
//		{
//			return;
//		}

//		// Token kontrolü
//		var token = context.HttpContext.Request.Cookies["JWTToken"];
//		if (string.IsNullOrEmpty(token))
//		{
//			// Loglama
//			Console.WriteLine("Token yok, login sayfasına yönlendiriliyor...");
//			context.Result = new RedirectToActionResult("Login", "Auth", null);
//			return;
//		}

//		try
//		{
//			var handler = new JwtSecurityTokenHandler();
//			var jwtToken = handler.ReadJwtToken(token);
//			var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

//			if (roleClaim == null)
//			{
//				context.Result = new RedirectToActionResult("Login", "Auth", null);
//				return;
//			}

//			var controllerName = context.ActionDescriptor.RouteValues["controller"];
//			var actionName = context.ActionDescriptor.RouteValues["action"];

//			// Admin ve SuperAdmin kontrolleri
//			if (controllerName == "Admin" && roleClaim != Role.Admin.ToString() && roleClaim != Role.SuperAdmin.ToString())
//			{
//				context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
//			}
//			// Diğer kontroller...
//		}
//		catch (Exception ex)
//		{
//			// Token okunamadı veya geçersiz
//			Console.WriteLine($"Token okunurken hata oluştu: {ex.Message}");
//			context.Result = new RedirectToActionResult("Login", "Auth", null);
//		}
//	}

//	public void OnActionExecuted(ActionExecutedContext context)
//	{
//	}
//}

public class SessionAuthorizationFilter : IActionFilter
{
	public void OnActionExecuting(ActionExecutingContext context)
	{
		// Login sayfasına erişimi kontrol etme
		if (context.ActionDescriptor.RouteValues["controller"] == "Auth" &&
			context.ActionDescriptor.RouteValues["action"] == "Login")
		{
			return;
		}

		// Setup action'ını filtre dışında tutma
		if (context.ActionDescriptor.RouteValues["controller"] == "Auth" &&
			context.ActionDescriptor.RouteValues["action"] == "Setup")
		{
			return;
		}


		// sqlBaglatinAyarlari action methodunu filtre dışında tutma
		if (context.ActionDescriptor.RouteValues["controller"] == "Settings" &&
			context.ActionDescriptor.RouteValues["action"] == "sqlBaglatinAyarlari")
		{
			return;
		}



		var token = context.HttpContext.Request.Cookies["JWTToken"];
		if (string.IsNullOrEmpty(token))
		{
			// Loglama
			Console.WriteLine("Token yok, login sayfasına yönlendiriliyor...");
			context.Result = new RedirectToActionResult("", "Auth", null);
		}
	}

	public void OnActionExecuted(ActionExecutedContext context)
	{
	}
}