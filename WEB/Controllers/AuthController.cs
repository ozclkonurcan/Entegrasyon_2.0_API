using WEB.Interfaces;
using WEB.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace WEB.Controllers
{
	public class AuthController : Controller
	{
		private readonly IConfiguration _configuration;
		private readonly IApiService _apiService;
		private readonly HttpClient _httpClient;

		public AuthController(IConfiguration configuration, IApiService apiService, HttpClient httpClient)
		{
			_configuration = configuration;
			_apiService = apiService;
			_httpClient = httpClient;
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> Login()
		{
			// Eğer kullanıcı zaten kimlik doğrulanmışsa, doğrudan ana sayfaya yönlendir.
			if (User.Identity.IsAuthenticated)
			{
				return RedirectToAction("Index", "Home");
			}

			// Gerekirse eskimiş token varsa temizleyin.
			if (HttpContext.Request.Cookies.ContainsKey("JWTToken"))
			{
				Response.Cookies.Delete("JWTToken");
			}

			// Diğer tablo ve bağlantı kontrollerinizi burada yapmaya devam edebilirsiniz...
			var isConnectionSuccess = await _apiService.CheckSqlConnectionAsync();
			var areTablesReady = await _apiService.CheckTablesAsync();
			if (!isConnectionSuccess)
			{
				TempData["ErrorMessage"] = "SQL Bağlantı problemi. Lütfen bağlantı ayarlarını kontrol edin.";
				return View();
			}
			if (!areTablesReady)
			{
				TempData["ErrorMessage"] = "Tablo kurulumu tamamlanmamış. Lütfen kurulumu tamamlayın.";
				TempData["ShowSetupButton"] = true;
				TempData["DisableLoginButton"] = true;
				return View();
			}

			return View();
		}

		[AllowAnonymous]
		[HttpPost("login")]
		public async Task<IActionResult> Login(LoginSettings loginSettings)
		{
			try
			{
				if (string.IsNullOrEmpty(loginSettings.Email) || string.IsNullOrEmpty(loginSettings.Password))
				{
					TempData["ErrorMessage"] = "Email veya şifre eksik.";
					return View();
				}

				var isConnectionSuccess = await _apiService.CheckSqlConnectionAsync();
				var areTablesReady = await _apiService.CheckTablesAsync();

				if (!isConnectionSuccess)
				{
					TempData["ErrorMessage"] = "SQL Bağlantı problemi. Lütfen bağlantı ayarlarını kontrol edin.";
					return View();
				}

				if (!areTablesReady)
				{
					TempData["ErrorMessage"] = "Tablo kurulumu tamamlanmamış. Lütfen kurulumu tamamlayın.";
					TempData["ShowSetupButton"] = true;
					TempData["DisableLoginButton"] = true;
					return View();
				}

				// IP adresini al ve loginSettings'e ata
				loginSettings.IpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";

				// API isteği gönder ve token'ları al
				var tokenResponse = await _apiService.LoginAsync(loginSettings);

				// JWT token ve refresh token'ı cookie'lere ekle
				Response.Cookies.Append("JWTToken", tokenResponse.Token, new CookieOptions
				{
					HttpOnly = true,
					Secure = false,
					SameSite = SameSiteMode.Strict,
					Expires = DateTime.UtcNow.AddHours(1)
				});
				Response.Cookies.Append("RefreshToken", tokenResponse.RefreshToken, new CookieOptions
				{
					HttpOnly = true,
					Secure = false,
					SameSite = SameSiteMode.Strict,
					Expires = DateTime.UtcNow.AddDays(30)
				});

				// ASP.NET Core Cookie Authentication ile oturumu başlatın.
				var claims = new List<Claim>
		{
			new Claim(ClaimTypes.Name, loginSettings.Email),
			new Claim("FullName", tokenResponse.FullName),
			new Claim(ClaimTypes.NameIdentifier, tokenResponse.Id.ToString()),
			new Claim(ClaimTypes.Role, tokenResponse.Role)
		};

				var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
				var authProperties = new AuthenticationProperties
				{
					IsPersistent = true,
					ExpiresUtc = DateTime.UtcNow.AddHours(1)
				};

				await HttpContext.SignInAsync(
					CookieAuthenticationDefaults.AuthenticationScheme,
					new ClaimsPrincipal(claimsIdentity),
					authProperties);

				HttpContext.Session.SetString("Email", loginSettings.Email);
				HttpContext.Session.SetString("FullName", tokenResponse.FullName);
				HttpContext.Session.SetInt32("Id", tokenResponse.Id);

				TempData["SuccessMessage"] = "Giriş başarılı!";
				return Redirect("/Home/Index");
				//return RedirectToAction("Index", "Home");
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = "HATA: " + ex.Message;
				return View();
			}
		}

		[HttpPost("logout")]
		public async Task<IActionResult> Logout()
		{
			try { await _apiService.LogoutAsync(); } catch { }
			Response.Cookies.Delete("JWTToken");
			Response.Cookies.Delete("RefreshToken");
			HttpContext.Session.Clear();
			TempData["SuccessMessage"] = "Çıkış yapıldı.";
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("", "Auth");
		}

		// Diğer aksiyonlar (Setup, RefreshToken vs.) aynı kalabilir.
		[HttpPost]
		public async Task<IActionResult> RefreshToken()
		{
			var refreshToken = Request.Cookies["RefreshToken"];
			if (string.IsNullOrEmpty(refreshToken))
			{
				return RedirectToAction("Login");
			}

			var refreshRequest = new RefreshTokenRequest
			{
				RefreshToken = refreshToken,
				IpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1"
			};

			try
			{
				var tokenResponse = await _apiService.RefreshTokenAsync(refreshRequest);

				Response.Cookies.Append("JWTToken", tokenResponse.Token, new CookieOptions
				{
					HttpOnly = true,
					Secure = false,
					SameSite = SameSiteMode.Strict,
					Expires = DateTime.UtcNow.AddHours(1)
				});

				Response.Cookies.Append("RefreshToken", tokenResponse.RefreshToken, new CookieOptions
				{
					HttpOnly = true,
					Secure = false,
					SameSite = SameSiteMode.Strict,
					Expires = DateTime.UtcNow.AddDays(30)
				});

				// Eğer isterseniz cookie authentication'ı da güncelleyebilirsiniz.
				// (Burada temel oturum bilgisi zaten SignIn esnasında oluşturuldu.)
				return RedirectToAction("Index", "Home");
			}
			catch
			{
				return RedirectToAction("Login");
			}
		}
	}
}


//using WEB.Interfaces;
//using WEB.Models;
//using WEB.Services;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;

//namespace WEB.Controllers;

//public class AuthController : Controller
//{
//	//private readonly LdapAuthenticationService _ldapService;
//	private readonly IConfiguration _configuration;
//	private readonly IApiService _apiService;
//	private readonly HttpClient _httpClient;

//	public AuthController(/*LdapAuthenticationService ldapService,*/ IConfiguration configuration, IApiService apiService, HttpClient httpClient)
//	{
//		//_ldapService = ldapService;
//		_configuration = configuration;
//		_apiService = apiService;
//		_httpClient = httpClient;
//	}
//	public IActionResult Index()
//	{
//		return View();
//	}

//	[AllowAnonymous]
//	[HttpGet]
//	public async Task<IActionResult> Login()
//	{
//		// Tablo kurulum kontrolü
//		var isConnectionSuccess = await _apiService.CheckSqlConnectionAsync();
//		var areTablesReady = await _apiService.CheckTablesAsync();
//		if (!isConnectionSuccess)
//		{
//			TempData["ErrorMessage"] = "SQL Bağlantı problemi. Lütfen bağlantı ayarlarını kontrol edin.";
//			return View();
//		}
//		if (!areTablesReady)
//		{
//			TempData["ErrorMessage"] = "Tablo kurulumu tamamlanmamış. Lütfen kurulumu tamamlayın.";
//			TempData["ShowSetupButton"] = true; // Kurulum butonunu aktifleştir
//			TempData["DisableLoginButton"] = true; // Giriş butonunu devre dışı bırak
//			return View();
//		}
//		// Token'ı kontrol et
//		var token = HttpContext.Request.Cookies["JWTToken"];
//		if (!string.IsNullOrEmpty(token))
//		{
//			// Token geçerliliğini API'ye sor
//			bool isValidToken = await _apiService.ValidateTokenAsync(token);

//			if (isValidToken)
//			{
//				// Token hala geçerliyse, ana sayfaya yönlendir
//				return RedirectToAction("Index", "Home");
//			}
//		}

//		// Token yoksa veya geçersizse, login sayfasını göster
//		return View();
//	}

//	[AllowAnonymous]
//	[HttpPost("login")]
//	public async Task<IActionResult> Login(LoginSettings loginSettings)
//	{
//		try
//		{
//			if (string.IsNullOrEmpty(loginSettings.Email) || string.IsNullOrEmpty(loginSettings.Password))
//			{
//				TempData["ErrorMessage"] = "Email veya şifre eksik.";
//				return View();
//			}

//			// Tablo kurulum kontrolü
//			var isConnectionSuccess = await _apiService.CheckSqlConnectionAsync();
//			var areTablesReady = await _apiService.CheckTablesAsync();

//			if (!isConnectionSuccess)
//			{
//				TempData["ErrorMessage"] = "SQL Bağlantı problemi. Lütfen bağlantı ayarlarını kontrol edin.";
//				return View();
//			}

//			if (!areTablesReady)
//			{
//				TempData["ErrorMessage"] = "Tablo kurulumu tamamlanmamış. Lütfen kurulumu tamamlayın.";
//				TempData["ShowSetupButton"] = true;
//				TempData["DisableLoginButton"] = true;
//				return View();
//			}

//			// IP adresini al ve loginSettings'e ata
//			loginSettings.IpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";

//			// API isteğini gönder
//			var tokenResponse = await _apiService.LoginAsync(loginSettings);

//			// Token'i cookie'de sakla
//			Response.Cookies.Append("JWTToken", tokenResponse.Token, new CookieOptions
//			{
//				HttpOnly = true,
//				Secure = false, // Localhost'ta HTTP kullanıyorsanız false yapın
//				SameSite = SameSiteMode.Strict,
//				Expires = DateTime.UtcNow.AddHours(1)
//			});

//			Response.Cookies.Append("RefreshToken", tokenResponse.RefreshToken, new CookieOptions
//			{
//				HttpOnly = true,
//				Secure = false, // Geliştirme aşamasında HTTP kullanıyorsanız false yapın, üretimde HTTPS kullanıyorsanız true yapın
//				SameSite = SameSiteMode.Strict,
//				Expires = DateTime.UtcNow.AddDays(30)
//			});

//			HttpContext.Session.SetString("Email", loginSettings.Email);
//			HttpContext.Session.SetString("FullName", tokenResponse.FullName);
//			HttpContext.Session.SetInt32("Id", tokenResponse.Id);

//			TempData["SuccessMessage"] = "Giriş başarılı!";
//			return RedirectToAction("Index", "Home");
//		}
//		catch (Exception ex)
//		{
//			TempData["ErrorMessage"] = "HATA: " + ex.Message;
//			return View();
//		}
//	}

//	//[AllowAnonymous]
//	//[HttpPost("login")]
//	//public async Task<IActionResult> Login(LoginSettings loginSettings)
//	//{
//	//	try
//	//	{

//	//		if (string.IsNullOrEmpty(loginSettings.Email) || string.IsNullOrEmpty(loginSettings.Password))
//	//		{
//	//			TempData["ErrorMessage"] = "Email veya şifre eksik.";
//	//			return View();
//	//		}


//	//		// Tablo kurulum kontrolü
//	//		var isConnectionSuccess = await _apiService.CheckSqlConnectionAsync();
//	//		var areTablesReady = await _apiService.CheckTablesAsync();

//	//		if (!isConnectionSuccess)
//	//		{
//	//			TempData["ErrorMessage"] = "SQL Bağlantı problemi. Lütfen bağlantı ayarlarını kontrol edin.";
//	//			return View();
//	//		}

//	//		if (!areTablesReady)
//	//		{
//	//			TempData["ErrorMessage"] = "Tablo kurulumu tamamlanmamış. Lütfen kurulumu tamamlayın.";
//	//			TempData["ShowSetupButton"] = true; // Kurulum butonunu aktifleştir
//	//			TempData["DisableLoginButton"] = true; // Giriş butonunu devre dışı bırak
//	//			return View();
//	//		}
//	//		// HttpClient'ı kullanarak API'ye istek gönder
//	//		var tokenResponse = await _apiService.LoginAsync(loginSettings);


//	//		// Token'i cookie'de sakla
//	//		Response.Cookies.Append("JWTToken", tokenResponse.Token, new CookieOptions
//	//		{
//	//			HttpOnly = true,
//	//			Secure = false, // Localhost'ta HTTP kullanıyorsan false yap
//	//			SameSite = SameSiteMode.Strict,
//	//			Expires = DateTime.UtcNow.AddHours(1)
//	//		});

//	//		HttpContext.Session.SetString("Email", loginSettings.Email);
//	//			HttpContext.Session.SetString("FullName", tokenResponse.FullName);
//	//			HttpContext.Session.SetInt32("Id", tokenResponse.Id);

//	//			TempData["SuccessMessage"] = "Giriş başarılı!";
//	//			return RedirectToAction("Index", "Home");

//	//	}
//	//	catch (Exception ex)
//	//	{
//	//		TempData["ErrorMessage"] = "HATA: " + ex.Message;
//	//		return View();
//	//	}
//	//}

//	[HttpPost("logout")]
//	public async Task<IActionResult> Logout()
//	{
//		try
//		{
//			await _apiService.LogoutAsync();
//		}
//		catch { }

//		Response.Cookies.Delete("JWTToken");
//		HttpContext.Session.Clear();
//		TempData["SuccessMessage"] = "Çıkış yapıldı.";
//		return RedirectToAction("", "Auth");
//	}




//	[HttpPost]
//	public async Task<IActionResult> Setup()
//	{
//		try
//		{
//			// Kurulum işlemini başlat
//			var isSetupSuccessful = await _apiService.SetupTable();

//			if (isSetupSuccessful)
//			{
//				return Json(new { success = true, message = "Kurulum başarıyla tamamlandı!" });
//			}
//			else
//			{
//				return Json(new { success = false, message = "Kurulum sırasında bir hata oluştu. Lütfen tekrar deneyin." });
//			}
//		}
//		catch (HttpRequestException ex)
//		{
//			// API ile iletişim sırasında bir hata oluştu
//			return Json(new { success = false, message = "Kurulum sırasında bir hata oluştu. API ile iletişim kurulamadı. Hata: " + ex.Message });
//		}
//		catch (Exception ex)
//		{
//			// Beklenmeyen bir hata oluştu
//			return Json(new { success = false, message = "Kurulum sırasında beklenmeyen bir hata oluştu. Hata: " + ex.Message });
//		}
//	}




//	[HttpPost]
//	public async Task<IActionResult> RefreshToken()
//	{
//		var refreshToken = Request.Cookies["RefreshToken"];
//		if (string.IsNullOrEmpty(refreshToken))
//		{
//			return RedirectToAction("Login");
//		}

//		var refreshRequest = new RefreshTokenRequest
//		{
//			RefreshToken = refreshToken,
//			IpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1"
//		};

//		try
//		{
//			var tokenResponse = await _apiService.RefreshTokenAsync(refreshRequest);

//			Response.Cookies.Append("JWTToken", tokenResponse.Token, new CookieOptions
//			{
//				HttpOnly = true,
//				Secure = false,
//				SameSite = SameSiteMode.Strict,
//				Expires = DateTime.UtcNow.AddHours(1)
//			});

//			Response.Cookies.Append("RefreshToken", tokenResponse.RefreshToken, new CookieOptions
//			{
//				HttpOnly = true,
//				Secure = false,
//				SameSite = SameSiteMode.Strict,
//				Expires = DateTime.UtcNow.AddDays(30)
//			});

//			return RedirectToAction("Index", "Home");
//		}
//		catch
//		{
//			return RedirectToAction("Login");
//		}
//	}




//}





public class LoginSettings
	{
		public string Email { get; set; }
		public string Password { get; set; }
	public string IpAddress { get; set; }  
}

public class LoggedResponse
{
	public int Id { get; set; }
	public string Email { get; set; }
	public string FullName { get; set; }
	public string Role { get; set; }
	public string Token { get; set; }
	public string RefreshToken { get; set; } 
}

public class TokenResponse
{
	public string Token { get; set; } // JWT Token
	public string FullName { get; set; } // Kullanıcının tam adı
	public string Email { get; set; } // Kullanıcının e-posta adresi
	public int Id { get; set; } // Kullanıcının ID'si
	public DateTime Expires { get; set; } // Token'ın son kullanma tarihi
}