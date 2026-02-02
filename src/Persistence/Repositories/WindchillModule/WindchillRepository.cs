using Application.Interfaces.WindchillModule;
using Domain.Entities.WindchillEntities;
using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Persistence.Repositories.WindchillModule;

public class WindchillRepository : IWindchillService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly string _server;
	private readonly string _username;
	private readonly string _password;

	public WindchillRepository(IHttpClientFactory httpClientFactory)
	{
		_httpClientFactory = httpClientFactory;
		Env.Load();
		_server = Env.GetString("Windchill_Server");
		_username = Env.GetString("Windchill_Username");
		_password = Env.GetString("Windchill_Password");
	}


	public async Task<WrsToken> GetTokenAsync()
	{
		var client = _httpClientFactory.CreateClient();
		var request = new HttpRequestMessage(HttpMethod.Get, $"https://{_server}/Windchill/servlet/odata/PTC/GetCSRFToken()");

		// Basic Authentication
		var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
		request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

		var response = await client.SendAsync(request);
		response.EnsureSuccessStatusCode();

		var responseContent = await response.Content.ReadAsStringAsync();
		// JSON'dan WrsToken'a dönüştürme işlemi (örnek olarak)
		var token = System.Text.Json.JsonSerializer.Deserialize<WrsToken>(responseContent);

		return token;
	}


	public Task<string> GetUserAsync()
	{
		throw new NotImplementedException();


	}
	public async Task<List<WTUsers>> GetFindUserAsync(string? searchTerm)
	{
		var client = _httpClientFactory.CreateClient();

		// CSRF token'ını al
		var token = await GetTokenAsync();
		if (token == null || string.IsNullOrEmpty(token.NonceValue))
		{
			throw new Exception("CSRF token alınamadı.");
		}

		// OData sorgusunu oluştur
		var filter = new List<string>();
		if (!string.IsNullOrEmpty(searchTerm))
		{
			// Hem FullName hem de EMail alanlarında filtreleme yap
			filter.Add($"startswith(FullName, '{searchTerm}') or startswith(EMail, '{searchTerm}')");
		}

		var filterQuery = string.Join(" or ", filter);
		var requestUrl = $"https://{_server}/Windchill/servlet/odata/PrincipalMgmt/Users?$select=ID,Name,EMail,FullName&$filter={filterQuery}";

		var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);

		// CSRF Token ve Basic Authentication ekle
		request.Headers.Add("CSRF_NONCE", token.NonceValue);
		var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
		request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authHeader);

		var response = await client.SendAsync(request);
		response.EnsureSuccessStatusCode();

		var responseContent = await response.Content.ReadAsStringAsync();

		// JSON'dan List<WTUsers>'a dönüştür
		var jsonDocument = JsonDocument.Parse(responseContent);
		var users = jsonDocument.RootElement
			.GetProperty("value")
			.EnumerateArray()
			.Select(u => new WTUsers
			{
				ID = u.GetProperty("ID").GetString(),
				Name = u.GetProperty("Name").GetString(),
				EMail = u.GetProperty("EMail").GetString(),
				FullName = u.GetProperty("FullName").GetString()
			})
			.ToList();

		return users;
	}

}
