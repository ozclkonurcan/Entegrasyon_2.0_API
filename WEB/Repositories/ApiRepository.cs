using WEB.Controllers;
using WEB.Controllers.Ayarlar;
using WEB.Interfaces;
using WEB.Models;
using WEB.Services;
using Microsoft.Extensions.Configuration;
using System.DirectoryServices.Protocols;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace WEB.Repositories;

public class ApiRepository : IApiService
{
	private readonly IConfiguration _configuration;
	private readonly IHttpClientFactory _clientFactory;
	private readonly IGetTokenService _tokenService;

	public ApiRepository(IConfiguration configuration, IHttpClientFactory clientFactory, IGetTokenService tokenService)
	{
		_configuration = configuration;
		_clientFactory = clientFactory;
		_tokenService = tokenService;
	}

	private string GetBaseUrl()
	{
		var httpUrl = _configuration["DesigntechSettings:httpUrl"];
		var portNumber = _configuration["DesigntechSettings:PortNumber"];
		return $"{httpUrl}{portNumber}";
	}

	private HttpClient CreateHttpClient()
	{
		var client = _clientFactory.CreateClient();

		client.BaseAddress = new Uri(GetBaseUrl());
		return client;
	}

	public async Task<T> GetAsync<T>(string endpoint)
	{
		using var client = CreateHttpClient();
		var token = await _tokenService.GetTokenAsync();
		client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		var response = await client.GetAsync(endpoint);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<T>();
	}



	public async Task<string> PostAsync<TRequest>(string endpoint, TRequest data)
	{
		using var client = CreateHttpClient();
		var token = await _tokenService.GetTokenAsync();
		client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		var response = await client.PostAsJsonAsync(endpoint, data);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStringAsync();
	}

	public async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
	{
		using var client = CreateHttpClient();
		var token = await _tokenService.GetTokenAsync();
		client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		var response = await client.PutAsJsonAsync(endpoint, data);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<TResponse>();
	}

	public async Task<bool> DeleteAsync(string endpoint)
	{
		using var client = CreateHttpClient();
		var token = await _tokenService.GetTokenAsync();
		client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		var response = await client.DeleteAsync(endpoint);
		return response.IsSuccessStatusCode;
	}

	

	public async Task<LoggedResponse> LoginAsync(LoginSettings loginSettings)
	{
		using var client = CreateHttpClient();

		var response = await client.PostAsJsonAsync("api/Auth/login", loginSettings);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<LoggedResponse>();
	}



	public async Task LogoutAsync()
	{
		using var client = CreateHttpClient();
		var logoutCommand = new { LogMessage = "MVC'den logout tetiklendi." };
		var response = await client.PostAsJsonAsync("api/Auth/logout", logoutCommand);
		response.EnsureSuccessStatusCode();
	}

	public async Task<bool> ValidateTokenAsync(string token)
	{
		using var client = CreateHttpClient();
		client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		var response = await client.GetAsync("api/Auth/validate-token");
		return response.IsSuccessStatusCode;
	}

	//public async Task<bool> ValidateTokenAsync(string token)
	//{
	//	using var client = CreateHttpClient();

	//	client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
	//	var response = await client.GetAsync("api/Auth/validate-token");
	//	return response.IsSuccessStatusCode;
	//}

	public async Task<bool> CheckTablesAsync()
	{
		try
		{
			// API'den tablo durumlarını çek
			using var client = CreateHttpClient();

			var response = await client.GetAsync("api/DatabaseManagement/TableControls");

			// Yanıt başarılı mı kontrol et
			response.EnsureSuccessStatusCode();

			// JSON'u List<TableStatus> türüne dönüştür
			var tables = await response.Content.ReadFromJsonAsync<List<TableStatus>>();

			// Tüm tabloların IsActive değeri true ise true döner
			return tables.All(t => t.IsActive);
		}
		catch (HttpRequestException)
		{
			// API çağrısı başarısızsa false döner
			return false;
		}
		catch (Exception)
		{
			// Diğer hatalar için false döner
			return false;
		}
	}


	public async Task<bool> SetupTable()
	{
		using var client = CreateHttpClient();
		var content = new StringContent(string.Empty); // Boş bir body gönder
		var response = await client.PostAsync("api/DatabaseManagement", content);
		return response.IsSuccessStatusCode;
	}

	public async Task<bool> CheckSqlConnectionAsync()
	{
		try
		{
			// API'den bağlantı durumunu çek
			using var client = CreateHttpClient();
			var response = await client.GetAsync("api/Connection/ConnectionControl");

			// Yanıt başarılı mı kontrol et
			response.EnsureSuccessStatusCode();

			// JSON'u bool türüne dönüştür
			var isActive = await response.Content.ReadFromJsonAsync<bool>();

			// Bağlantı durumunu döndür
			return isActive;
		}
		catch (HttpRequestException)
		{
			// API çağrısı başarısızsa false döner
			return false;
		}
		catch (Exception)
		{
			// Diğer hatalar için false döner
			return false;
		}
	}



	public async Task<bool> CheckWindchillConnectionAsync()
	{
		try
		{
			using var client = CreateHttpClient();

			var response = await client.GetAsync("api/Windchill/WindchillToken");

			// Yanıt başarılı mı kontrol et
			response.EnsureSuccessStatusCode();
			return response.IsSuccessStatusCode;
		}
		catch
		{
			return false;
		}
	}

	public async Task<List<WTUsers>> SearchUsersAsync(string searchTerm)
	{
		using var client = CreateHttpClient();
		var token = await _tokenService.GetTokenAsync();
		client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
		var endpoint = $"api/Windchill/WindchillUsers?searchTerm={searchTerm}";
		var response = await client.GetAsync(endpoint);
		response.EnsureSuccessStatusCode();

		var content = await response.Content.ReadAsStringAsync();
		var jsonDocument = JsonDocument.Parse(content);
		var users = jsonDocument.RootElement
			.EnumerateArray()
			.Select(u => new WTUsers
			{
				FullName = u.GetProperty("fullName").GetString(), // "fullName" doğru
				EMail = u.GetProperty("eMail").GetString()       // "eMail" olarak güncellendi
			})
			.ToList();

		return users;
	}

	public async Task<bool> CheckSqlConnection(SqlConnectionModel model)
	{
		try
		{
			// SQL bağlantısını kontrol etmek için API'ye istek gönder
			using var client = CreateHttpClient();
			var response = await client.PostAsJsonAsync("api/Connection/ConnectionControlWithModel", model);

			// Yanıt başarılı mı kontrol et
			response.EnsureSuccessStatusCode();

			// JSON'u bool türüne dönüştür
			var isActive = await response.Content.ReadFromJsonAsync<bool>();

			// Bağlantı durumunu döndür
			return isActive;
		}
		catch (HttpRequestException ex)
		{
			// API çağrısı başarısızsa false döner
			Console.Error.WriteLine($"HTTP Request Error: {ex.Message}");
			return false;
		}
		catch (Exception ex)
		{
			// Diğer hatalar için false döner
			Console.Error.WriteLine($"Error: {ex.Message}");
			return false;
		}
	}

	public async Task<bool> CheckApiConnectionAsync(ApiConnectionModel model)
	{
		try
		{
			using (var httpClient = new HttpClient())
			{
				var fullUrl = $"{model.ApiUrl}{model.ApiPort}/api/connection/check";
				var response = await httpClient.GetAsync(fullUrl);
				return response.IsSuccessStatusCode;
			}
		}
		catch
		{
			return false;
		}
	}

	public async Task<bool> CheckWindchillConnection(WindchillConnectionSettings model)
	{
		try
		{
			// SQL bağlantısını kontrol etmek için API'ye istek gönder
			using var client = CreateHttpClient();
			var response = await client.PostAsJsonAsync("api/Connection/CheckWindchillConnection", model);

			// Yanıt başarılı mı kontrol et
			// Yanıt başarılı mı kontrol et
			response.EnsureSuccessStatusCode();

			// Yanıtı JSON olarak oku
			var responseData = await response.Content.ReadFromJsonAsync<ApiResponse>();

			// API'nin döndüğü success değerini kontrol et
			return responseData?.Success ?? false;
		}
		catch
		{
			return false;
		}
	}

	public async Task<LoggedResponse> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
	{
		using var client = CreateHttpClient();
		// Eğer ipAddress'ı dinamik olarak göndermiyorsanız, refreshTokenRequest.IpAddress'ı önceden set ettiğinizden emin olun.
		var response = await client.PostAsJsonAsync("api/Auth/refresh-token", refreshTokenRequest);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<LoggedResponse>();
	}

}



public class TableStatus
{
	public string TableName { get; set; }
	public bool IsActive { get; set; }
}

public class ApiResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }
}