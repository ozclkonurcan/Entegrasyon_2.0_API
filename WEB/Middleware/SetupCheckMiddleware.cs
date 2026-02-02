using WEB.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace WEB.Middleware
{
	public class SetupCheckMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<SetupCheckMiddleware> _logger;

		public SetupCheckMiddleware(RequestDelegate next, ILogger<SetupCheckMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			try
			{
				// İzin verilen yolların listesi
				var allowedPaths = new[]
				{
			"/Setup",
			"/Settings/CheckSqlConnection",
			"/Settings/CheckApiConnection",
			"/Settings/CheckWindchillConnection",
			"/Settings/sqlBaglantiAyarlari",
			"/Settings/windchillBaglantiAyarlari",
			"/Auth/Login",
			"/Auth/RefreshToken", // RefreshToken endpoint'ini de ekleyin
            "/Error",
			"/favicon.ico",
			"/css",
			"/js",
			"/lib"
		};

				// Statik dosyalar ve izin verilen yollar için middleware'i atla
				if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
					allowedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
				{
					await _next(context);
					return;
				}

				// API servisini kontrol et
				var apiService = context.RequestServices.GetRequiredService<IApiService>();

				// Bağlantı kontrolleri
				var isConnectionSuccess = await apiService.CheckSqlConnectionAsync();
				var areTablesReady = await apiService.CheckTablesAsync();

				if (!isConnectionSuccess || !areTablesReady)
				{
					_logger.LogWarning("Bağlantı kontrolü başarısız: SQL Connection: {isConnectionSuccess}, Tables Ready: {areTablesReady}",
						isConnectionSuccess, areTablesReady);

					// Oturum temizleme
					await ClearSession(context);

					// Setup sayfasına yönlendirme
					if (!context.Response.HasStarted)
					{
						context.Response.Redirect("/Setup/Index", false);
						return;
					}
				}

				await _next(context);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "SetupCheckMiddleware'de hata oluştu");

				if (!context.Response.HasStarted)
				{
					await ClearSession(context);
					context.Response.Redirect("/Setup/Index", false);
				}
			}
		}

		private async Task ClearSession(HttpContext context)
		{
			try
			{
				if (!string.IsNullOrEmpty(context.Session.GetString("Email")))
				{
					context.Session.Clear();
					context.Response.Cookies.Delete("JWTToken");

					// Varsa diğer authentication cookie'lerini de temizle
					await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Oturum temizleme sırasında hata oluştu");
			}
		}

		//public async Task InvokeAsync(HttpContext context)
		//{
		//	try
		//	{
		//		// Eğer istek bir AJAX isteği ise veya /Setup/Index sayfasından geliyorsa, middleware'i atla
		//		if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
		//			  context.Request.Path.StartsWithSegments("/Setup") ||
		//			  context.Request.Path.StartsWithSegments("/Settings/CheckSqlConnection") ||
		//			  context.Request.Path.StartsWithSegments("/Settings/CheckApiConnection") ||
		//			  context.Request.Path.StartsWithSegments("/Settings/CheckWindchillConnection") ||
		//			  context.Request.Path.StartsWithSegments("/Settings/sqlBaglantiAyarlari") ||
		//			  context.Request.Path.StartsWithSegments("/Settings/windchillBaglantiAyarlari") ||
		//			  context.Request.Path.StartsWithSegments("/"))
		//			  //context.Request.Path.StartsWithSegments("/Auth/Login"))
		//		{
		//			await _next(context);
		//			return;
		//		}

		//		// Scoped servisi HttpContext üzerinden al
		//		var apiService = context.RequestServices.GetRequiredService<IApiService>();

		//		// SQL bağlantısını kontrol et
		//		var isConnectionSuccess = await apiService.CheckSqlConnectionAsync();
		//		var areTablesReady = await apiService.CheckTablesAsync();

		//		if (!isConnectionSuccess || !areTablesReady)
		//		{
		//			// Kullanıcı oturum açmışsa, oturumu kapat
		//			if (!string.IsNullOrEmpty(context.Session.GetString("Email")))
		//			{
		//				context.Session.Clear(); // Oturumu temizle
		//				context.Response.Cookies.Delete("JWTToken"); // Token çerezini sil
		//			}

		//			// Kullanıcıyı ayarlar sayfasına yönlendir
		//			context.Response.Redirect("/Setup/Index");
		//			return;
		//		}



		//		// Her şey tamamsa, bir sonraki middleware'e geç
		//		await _next(context);
		//	}
		//	catch (Exception ex)
		//	{
		//		// Hata durumunda kullanıcıyı ayarlar sayfasına yönlendir
		//		context.Response.Redirect("/Setup/Index");
		//	}
		//}


	}
}
