using WEB.Hubs;
using WEB.Interfaces;
using WEB.Models;
using WEB.Models.Enums;
using WEB.Models.PaginationModels;
using WEB.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Abstractions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
 
namespace WEB.Controllers.Ayarlar;

public class SettingsController : BaseController
{
	private readonly HttpClient _httpClient;

	private readonly IConfigurationRoot _configurationRoot;
	private readonly IApiService _apiService;

	//private readonly IHubContext<AppHub> _hubContext;
	//private readonly LogService _logService;

	public SettingsController(HttpClient httpClient, IConfiguration configuration, IApiService apiService/* ,IHubContext<AppHub> hubContext, LogService logService*/) : base(configuration)
	{
		_httpClient = httpClient;
		//_configuration = configuration;
		_configurationRoot = (IConfigurationRoot)configuration;
		_apiService = apiService;
		//_hubContext = hubContext;
		//_logService = logService;
	}
	//[Authorize(Roles = "SuperAdmin,Admin")]
	[Authorize(Roles = "SuperAdmin,Admin")]
	public async Task<IActionResult> Index()
	{
		// Örneğin, API'den modül ayarlarını çekmek:
		var moduleSettings = await _apiService.GetAsync<List<ModuleSettingsViewModel>>("api/ModuleSettings");
		// Rol ayarları için benzer şekilde
		var roleMappings = await _apiService.GetAsync<List<RoleMappingViewModel>>("api/RoleMappings");

		var model = new SettingsIndexViewModel
		{
			ModuleSettings = moduleSettings,
			RoleMappings = roleMappings
		};

		return View(model);
	}

	// Modül ayarlarını kaydetmek için (POST)
	[HttpPost]
	public async Task<IActionResult> SaveModuleSettings([FromBody] ModuleSettingsViewModel model)
	{
		if (ModelState.IsValid)
		{
			if (model.Id > 0)
			{
				// Güncelleme için explicit generic tip belirtelim (örneğin, ModuleSettingsViewModel dönmesini bekliyorsak)
				await _apiService.PutAsync<ModuleSettingsViewModel, ModuleSettingsViewModel>($"api/ModuleSettings/{model.Id}", model);
			}
			else
			{
				await _apiService.PostAsync("api/ModuleSettings", model);
			}
			return Ok(new { success = true, message = "Modül ayarları başarıyla kaydedildi." });
		}
		return BadRequest(ModelState);
	}

	// Rol ekleme/düzenleme işlemleri için post action örneği
	[HttpPost]
	public async Task<IActionResult> SaveRoleMapping([FromBody] RoleMappingViewModel model)
	{
		if (ModelState.IsValid)
		{
			// Eğer model.Id > 0 ise güncelleme, yoksa yeni kayıt
			if (model.Id > 0)
			{
				// Tip argümanlarını açıkça belirtiyoruz:
				var updatedRole = await _apiService.PutAsync<RoleMappingViewModel, RoleMappingViewModel>($"api/RoleMappings/{model.Id}", model);
				if (updatedRole == null)
				{
					// API'den yanıt gelmiyorsa, hata loglayabilir veya farklı bir dönüş yapabilirsiniz.
					TempData["ErrorMessage"] = "Rol güncelleme işlemi sırasında yanıt alınamadı.";
				}
			}
			else
			{
				var result = await _apiService.PostAsync("api/RoleMappings", model);
				if (string.IsNullOrEmpty(result))
				{
					TempData["ErrorMessage"] = "Rol ekleme işlemi sırasında yanıt alınamadı.";
				}
			}
			return RedirectToAction("Index");
		}
		return View("Index", model);
	}

	[HttpGet]
	public async Task<IActionResult> GetProcessTags()
	{
		// API endpoint'i, örneğin Web API projesinde /api/RoleProcessTags
		var tags = await _apiService.GetAsync<List<RoleProcessTagDto>>("api/RoleProcessTags");
		return Json(tags);
	}

	#region Log Ayarları


	[Authorize(Roles = "SuperAdmin,Admin")]
	[HttpGet]
	public async Task<IActionResult> Log()
	{
		// Sayfa ilk açıldığında bugünün tarihini al
		var today = DateTime.Today;
		return await LogByDate(today,null,null); // LogByDate action'ını çağır
	}
	[Authorize(Roles = "SuperAdmin,Admin")]
	[HttpGet]
	public async Task<IActionResult> LogByDate(DateTime date, string? level = null, string? kullaniciAdi = null)
	{
		try
		{
			// API'ye gönderilecek sorgu parametrelerini oluştur
			var queryParams = new Dictionary<string, string>
		{
			{ "date", date.ToString("yyyy-MM-dd") }
		};

			if (!string.IsNullOrEmpty(level))
			{
				queryParams.Add("level", level);
			}

			if (!string.IsNullOrEmpty(kullaniciAdi))
			{
				queryParams.Add("kullaniciAdi", kullaniciAdi);
			}

			// Query parametrelerini URL'ye ekle
			var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={kvp.Value}"));
			var apiUrl = $"api/AuditLogs/by-date?{queryString}";

			// API'den logları çek
			var logResponse = await _apiService.GetAsync<LogResponse>(apiUrl);

			// Logları ters sırala (en yeni log en üstte)
			var sortedLogs = logResponse.Logs.OrderByDescending(l => l.TimeStamp).ToList();

			// ViewBag'e logları ve diğer bilgileri ekle
			ViewBag.logResponse = sortedLogs;
			ViewBag.SelectedDate = date;
			ViewBag.TotalLogs = sortedLogs.Count;
			ViewBag.SelectedLevel = level;
			ViewBag.SelectedKullaniciAdi = kullaniciAdi;

			// Yeni loglar geldiğinde SignalR ile bildirim gönder (isteğe bağlı)
			// await _hubContext.Clients.All.SendAsync("SendLogUpdate");

			return View("Log");
		}
		catch (Exception ex)
		{
			// Hata durumunda ViewBag'e hata mesajını ve boş log listesini ekle
			ViewBag.logResponse = new List<LogDto>();
			ViewBag.ErrorMessage = "Loglar alınırken bir hata oluştu: " + ex.Message;
			ViewBag.TotalLogs = 0;
			return View("Log");
		}
	}
	//[Authorize]
	//[HttpGet]
	//public async Task<IActionResult> LogByDate(DateTime date,string? level,string? kullaniciAdi)
	//{
	//	try
	//	{
	//		var logResponse = await _apiService.GetAsync<LogResponse>($"api/AuditLogs/by-date?date={date:yyyy-MM-dd}");

	//		// Logları ters sırala (en yeni log en üstte)
	//		var sortedLogs = logResponse.Logs.OrderByDescending(l => l.TimeStamp).ToList();

	//		ViewBag.logResponse = sortedLogs;
	//		ViewBag.SelectedDate = date;
	//		ViewBag.TotalLogs = sortedLogs.Count;

	//		// Yeni loglar geldiğinde SignalR ile bildirim gönder
	//		//await _hubContext.Clients.All.SendAsync("SendLogUpdate");

	//		return View("Log");
	//	}
	//	catch (Exception ex)
	//	{
	//		ViewBag.logResponse = new List<LogDto>();
	//		ViewBag.ErrorMessage = "Loglar alınırken bir hata oluştu.";
	//		ViewBag.TotalLogs = 0;
	//		return View("Log");
	//	}
	//}

	#endregion

	[HttpPost]
	public async Task<IActionResult> windchillBaglantiAyarlari([FromBody] WindchillConnectionSettings model)
	{
		try
		{	
				var ApiUrl = _configurationRoot["DesigntechSettings:httpUrl"];
				var ApiPort = _configurationRoot["DesigntechSettings:PortNumber"];
		
			// API'ye istek gönder
			using (var httpClient = new HttpClient())
			{
				var request = new HttpRequestMessage(HttpMethod.Put, $"{ApiUrl}{ApiPort}/api/Connection/WindchillConnection");
				var content = new StringContent(
					System.Text.Json.JsonSerializer.Serialize(model),
					System.Text.Encoding.UTF8,
					"application/json"
				);
				request.Content = content;

				var response = await httpClient.SendAsync(request);

				if (response.IsSuccessStatusCode)
				{
					// Başarılıysa, kullanıcıyı Login sayfasına yönlendir
					return Json(new { success = true, message = "Windchill bağlantı ayarları başarıyla kaydedildi!" });
				}
				else
				{
					return Json(new { success = false, message = "Windchill bağlantı ayarları kaydedilirken bir hata oluştu." });
				}
			}
		}
		catch (Exception ex)
		{
			return Json(new { success = false, message = $"Hata oluştu: {ex.Message}" });
		}
	}





	[HttpPost]
	public async Task<IActionResult> sqlBaglantiAyarlari([FromBody] AllConnectionSettingsRequest requestModel)
	{
		try
		{
			var sqlModel = requestModel.SqlModel;
			var apiModel = requestModel.ApiModel;
			var windchillModel = requestModel.WindchillModel;

			// API URL ve port bilgilerini appsettings.json'dan al veya güncelle
			if (string.IsNullOrEmpty(apiModel.ApiPort) && string.IsNullOrEmpty(apiModel.ApiUrl))
			{
				apiModel.ApiUrl = _configurationRoot["DesigntechSettings:httpUrl"];
				apiModel.ApiPort = _configurationRoot["DesigntechSettings:PortNumber"];
			}
			else
			{
				// appsettings.json dosyasını oku ve güncelle
				var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
				var appSettingsJson = System.IO.File.ReadAllText(appSettingsPath);

				var jsonDocument = JsonDocument.Parse(appSettingsJson);
				var root = jsonDocument.RootElement;

				var designtechSettings = root.GetProperty("DesigntechSettings");
				var updatedDesigntechSettings = new Dictionary<string, string>
			{
				{ "httpUrl", apiModel.ApiUrl },
				{ "PortNumber", apiModel.ApiPort }
			};

				// JSON'u güncelle
				var updatedJson = JsonSerializer.Serialize(new
				{
					Logging = root.GetProperty("Logging"),
					Jwt = root.GetProperty("Jwt"),
					DesigntechSettings = updatedDesigntechSettings,
					AllowedHosts = root.GetProperty("AllowedHosts").GetString()
				}, new JsonSerializerOptions { WriteIndented = true });

				// Dosyaya yeni değerleri yaz
				System.IO.File.WriteAllText(appSettingsPath, updatedJson);

				// Configuration'ı yeniden yükle (opsiyonel)
				_configurationRoot.Reload();
			}

			// API'ye SQL bağlantı ayarlarını gönder
			using (var httpClient = new HttpClient())
			{
				var sqlRequest = new HttpRequestMessage(HttpMethod.Put, $"{apiModel.ApiUrl}{apiModel.ApiPort}/api/Connection");
				var sqlContent = new StringContent(
					JsonSerializer.Serialize(sqlModel),
					Encoding.UTF8,
					"application/json"
				);
				sqlRequest.Content = sqlContent;

				var sqlResponse = await httpClient.SendAsync(sqlRequest);

				if (!sqlResponse.IsSuccessStatusCode)
				{
					return Json(new { success = false, message = "SQL bağlantı ayarları kaydedilirken bir hata oluştu." });
				}

				// API'ye Windchill bağlantı ayarlarını gönder
				var windchillRequest = new HttpRequestMessage(HttpMethod.Put, $"{apiModel.ApiUrl}{apiModel.ApiPort}/api/Connection/WindchillConnection");
				var windchillContent = new StringContent(
					JsonSerializer.Serialize(windchillModel),
					Encoding.UTF8,
					"application/json"
				);
				windchillRequest.Content = windchillContent;

				var windchillResponse = await httpClient.SendAsync(windchillRequest);

				if (!windchillResponse.IsSuccessStatusCode)
				{
					return Json(new { success = false, message = "Windchill bağlantı ayarları kaydedilirken bir hata oluştu." });
				}
			}

			// Tabloların hazır olup olmadığını kontrol et
			var areTablesReady = await _apiService.CheckTablesAsync();
			if (!areTablesReady)
			{
				var isSetupSuccessful = await _apiService.SetupTable();
				if (!isSetupSuccessful)
				{
					return Json(new { success = false, message = "Tablo kurulumu sırasında bir hata oluştu." });
				}
			}

			// Başarılıysa, kullanıcıyı Login sayfasına yönlendir
			return Json(new { success = true, redirectUrl = "/Auth/Login", message = "Tüm bağlantı ayarları başarıyla kaydedildi!" });
		}
		catch (Exception ex)
		{
			return Json(new { success = false, message = $"Hata oluştu: {ex.Message}" });
		}
	}



	[Authorize(Roles = "SuperAdmin,Admin")]
	[HttpPost]
	public async Task<IActionResult> KullaniciEkle([FromBody] KullaniciModel model)
	{
		try
		{
			if (model == null || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
			{
				TempData["ErrorMessage"] = "Email veya şifre eksik.";
				return BadRequest(TempData["ErrorMessage"]);
			}

			var response = await _apiService.PostAsync("api/Users", model);

			if (!string.IsNullOrEmpty(response))
			{

				TempData["SuccessMessage"] = "Kullanıcı başarıyla eklendi.";
				return Ok(TempData["ErrorMessage"]);
			}
			else
			{
				TempData["ErrorMessage"] = "Kullanıcı eklenirken bir hata oluştu.";
				return BadRequest(TempData["ErrorMessage"]);
			}
		}
		catch (HttpRequestException ex)
		{
			TempData["ErrorMessage"] = "API çağrısı sırasında bir hata oluştu: " + ex.Message;
			return StatusCode(500, TempData["ErrorMessage"]);
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = "Bir hata oluştu: " + ex.Message;
			return StatusCode(500, TempData["ErrorMessage"]);
		}
	}

	

	[Authorize(Roles = "SuperAdmin,Admin")]
	[HttpGet]
	public async Task<IActionResult> KullaniciAra(string query)
	{
		try
		{
			if (string.IsNullOrEmpty(query))
			{
				TempData["ErrorMessage"] = "Arama sorgusu boş olamaz.";
				return BadRequest(TempData["ErrorMessage"]);
			}

			var users = await _apiService.SearchUsersAsync(query);

			if (users.Any())
			{
				TempData["SuccessMessage"] = "Kullanıcılar başarıyla bulundu.";
				return Ok(users.Select(u => new { u.FullName, u.EMail }));
			}
			else
			{
				TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
				return NotFound(TempData["ErrorMessage"]);
			}
		}
		catch (Exception ex)
		{
			TempData["ErrorMessage"] = "Bir hata oluştu: " + ex.Message;
			return StatusCode(500, TempData["ErrorMessage"]);
		}
	}


	[AllowAnonymous]
	[HttpPost]
	public async Task<IActionResult> CheckSqlConnection([FromBody] SqlConnectionModel model)
	{
		try
		{
			// SQL bağlantısını kontrol et
			var isConnectionSuccess = await _apiService.CheckSqlConnection(model);
			return Ok(new { success = isConnectionSuccess, message = isConnectionSuccess ? "SQL bağlantısı başarılı!" : "SQL bağlantısı başarısız." });
		}
		catch (Exception ex)
		{
			return Ok(new { success = false, message = ex.Message });
		}
	}

	[AllowAnonymous]
	[HttpPost]
	public async Task<IActionResult> CheckApiConnection([FromBody] ApiConnectionModel model)
	{
		try
		{
			// API bağlantısını kontrol et
			var isConnectionSuccess = await _apiService.CheckApiConnectionAsync(model);
			return Ok(new { success = isConnectionSuccess, message = isConnectionSuccess ? "API bağlantısı başarılı!" : "API bağlantısı başarısız." });
		}
		catch (Exception ex)
		{
			return Ok(new { success = false, message = ex.Message });
		}
	}

		[AllowAnonymous]
	[HttpPost]
	public async Task<IActionResult> CheckWindchillConnection([FromBody] WindchillConnectionSettings model)
	{
		try
		{
			// API bağlantısını kontrol et
			var isConnectionSuccess = await _apiService.CheckWindchillConnection(model);
			return Ok(new { success = isConnectionSuccess, message = isConnectionSuccess ? "Windchill bağlantısı başarılı!" : "Windchill bağlantısı başarısız." });
		}
		catch (Exception ex)
		{
			return Ok(new { success = false, message = ex.Message });
		}
	}





	[HttpGet("WTPartLogs")]
	public async Task<IActionResult> WTPartLog()
	{
		return View();
	}

	[HttpGet("WTPartLogs/GetLogs")]
	public async Task<IActionResult> GetLogs(string? searchQuery, DateTime? startDate, DateTime? endDate, int pageIndex = 0, int pageSize = 10)
	{
		string apiUrl = $"api/WTParts/getlistalllogs?pageIndex={pageIndex}&pageSize={pageSize}";

		if (!string.IsNullOrEmpty(searchQuery))
			apiUrl += $"&searchQuery={searchQuery}";

		if (startDate.HasValue)
			apiUrl += $"&startDate={startDate.Value:yyyy-MM-dd}";

		if (endDate.HasValue)
			apiUrl += $"&endDate={endDate.Value:yyyy-MM-dd}";

		var logs = await _apiService.GetAsync<GetListResponse<WTPartLogViewModel>>(apiUrl);
		return Json(logs);
	}

	[HttpGet("WTPartAlternateLogs")]
	public async Task<IActionResult> WTPartAlternateLog()
	{
		return View();
	}

	[HttpGet("WTPartAlternateLogs/GetLogs")]
	public async Task<IActionResult> GetAlternateLogs(string? searchQuery, DateTime? startDate, DateTime? endDate, int pageIndex = 0, int pageSize = 10)
	{
		string apiUrl = $"api/WTParts/getlistallalternatelogs?pageIndex={pageIndex}&pageSize={pageSize}";

		if (!string.IsNullOrEmpty(searchQuery))
			apiUrl += $"&searchQuery={searchQuery}";

		if (startDate.HasValue)
			apiUrl += $"&startDate={startDate.Value:yyyy-MM-dd}";

		if (endDate.HasValue)
			apiUrl += $"&endDate={endDate.Value:yyyy-MM-dd}";

		var logs = await _apiService.GetAsync<GetListResponse<WTPartAlternateLogViewModel>>(apiUrl);
		return Json(logs);
	}


}





public class DesigntechSettings
{
	public string httpUrl { get; set; }
	public string PortNumber { get; set; }
}

public class ConnectionSettingsRequest
{
	public SqlConnectionSettings Model { get; set; }
	public ApiConnectionSettings ApiModel { get; set; }
}

public class ApiConnectionSettings
{
	public string ApiUrl { get; set; }
	public string ApiPort { get; set; }
}


public class SqlConnectionSettings
{
	public string Server { get; set; }
	public string Database { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }
	public string Schema { get; set; }
}

public class SettingsModel
{
	public string SqlConnectionString { get; set; }
	public string ApiUrl { get; set; }
	public int ApiPort { get; set; }
}

public class ODataResponse
{
	[JsonPropertyName("@odata.context")]
	public string ODataContext { get; set; }

	[JsonPropertyName("value")]
	public List<User> Value { get; set; }
}

public class User
{
	[JsonPropertyName("ID")]
	public string ID { get; set; }

	[JsonPropertyName("EMail")]
	public string EMail { get; set; }

	[JsonPropertyName("FullName")]
	public string FullName { get; set; }
}

public class KullaniciModel
{
	public string FullName { get; set; }
	public string Email { get; set; }
	public string Password { get; set; }
	public int Role { get; set; }
}


public class LogResponse
{
	public List<LogDto> Logs { get; set; }
}

public class LogDto
{
	public int Id { get; set; }
	public string Message { get; set; }
	public string MessageTemplate { get; set; }
	public string Level { get; set; }
	public DateTime TimeStamp { get; set; }
	public string Exception { get; set; }
	public string Properties { get; set; }
	public string TetiklenenFonksiyon { get; set; }
	public string KullaniciAdi { get; set; }
	public string HataMesaji { get; set; }
}


public class AllConnectionSettingsRequest
{
	public SqlConnectionSettings SqlModel { get; set; }
	public ApiConnectionSettings ApiModel { get; set; }
	public WindchillConnectionSettings WindchillModel { get; set; }
}



