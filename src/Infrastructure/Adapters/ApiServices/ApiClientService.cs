using Application.Interfaces.ApiService;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Adapters.ApiServices;

public class ApiClientService : IApiClientService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IConfiguration _configuration;
	private readonly ILogger<ApiClientService> _logger;

	public ApiClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ApiClientService> logger)
	{
		_httpClientFactory = httpClientFactory;
		_configuration = configuration;
		_logger = logger;
	}

	private HttpClient CreateClient()
	{
		var client = _httpClientFactory.CreateClient("WindchillAPI");

		Env.Load();
		var server = Env.GetString("Windchill_Server");
		var username = Env.GetString("Windchill_Username");
		var password = Env.GetString("Windchill_Password");

		if (!server.StartsWith("http://") && !server.StartsWith("https://"))
		{
			server = "https://" + server;
		}
		client.BaseAddress = new Uri(server);

		// Basic Authentication header'ını oluşturup ekliyoruz.
		var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

		// Timeout ve connection ayarları
		client.Timeout = TimeSpan.FromMinutes(5);
		client.DefaultRequestHeaders.ConnectionClose = false; // Keep-Alive
		client.DefaultRequestHeaders.Add("Keep-Alive", "timeout=300, max=1000");

		return client;
	}

	public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string>? headers = null)
	{
		var retryPolicy = Policy
			.Handle<HttpRequestException>()
			.Or<TaskCanceledException>()
			.Or<SocketException>()
			.WaitAndRetryAsync(
				retryCount: 3,
				sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // 2, 4, 8 saniye
				onRetry: (outcome, timespan, retryCount, context) =>
				{
					_logger.LogWarning("Windchill API retry {RetryCount}/3 - {Endpoint} - Bekleme: {Delay}s",
						retryCount, endpoint, timespan.TotalSeconds);
				});

		return await retryPolicy.ExecuteAsync(async () =>
		{
			using var client = CreateClient();

			// Opsiyonel header'ları ekleyelim.
			if (headers != null)
			{
				foreach (var header in headers)
				{
					if (!client.DefaultRequestHeaders.Contains(header.Key))
					{
						client.DefaultRequestHeaders.Add(header.Key, header.Value);
					}
				}
			}

			try
			{
				_logger.LogDebug("Windchill API isteği: {Endpoint}", endpoint);

				var response = await client.GetAsync(endpoint);
				response.EnsureSuccessStatusCode();

				// Eğer T tipi string ise, ham yanıtı döndürelim.
				if (typeof(T) == typeof(string))
				{
					var stringResponse = await response.Content.ReadAsStringAsync();
					return (T)(object)stringResponse;
				}
				else
				{
					return await response.Content.ReadFromJsonAsync<T>();
				}
			}
			catch (HttpRequestException ex) when (ex.InnerException is SocketException socketEx)
			{
				_logger.LogError("Windchill bağlantı hatası: {Endpoint} - {Error}", endpoint, socketEx.Message);
				throw;
			}
			catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
			{
				_logger.LogError("Windchill timeout hatası: {Endpoint}", endpoint);
				throw;
			}
		});
	}

	public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
	{
		var retryPolicy = Policy
			.Handle<HttpRequestException>()
			.Or<TaskCanceledException>()
			.WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(retryAttempt * 2));

		return await retryPolicy.ExecuteAsync(async () =>
		{
			using var client = CreateClient();
			var response = await client.PostAsJsonAsync(endpoint, data);
			response.EnsureSuccessStatusCode();
			return await response.Content.ReadFromJsonAsync<TResponse>();
		});
	}

	public async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
	{
		using var client = CreateClient();
		var response = await client.PutAsJsonAsync(endpoint, data);
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadFromJsonAsync<TResponse>();
	}

	public async Task<bool> DeleteAsync(string endpoint)
	{
		using var client = CreateClient();
		var response = await client.DeleteAsync(endpoint);
		return response.IsSuccessStatusCode;
	}
}


//using Application.Interfaces.ApiService;
//using DotNetEnv;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http.Headers;
//using System.Net.Http.Json;
//using System.Text;
//using System.Threading.Tasks;

//namespace Infrastructure.Adapters.ApiServices;

//public class ApiClientService : IApiClientService
//{

//		private readonly IHttpClientFactory _httpClientFactory;
//		private readonly IConfiguration _configuration;

//		public ApiClientService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
//		{
//			_httpClientFactory = httpClientFactory;
//			_configuration = configuration;
//		}

//		private HttpClient CreateClient()
//		{
//			var client = _httpClientFactory.CreateClient();

//			Env.Load();
//			var server = Env.GetString("Windchill_Server");       
//			var username = Env.GetString("Windchill_Username");    
//			var password = Env.GetString("Windchill_Password");     

//			if (!server.StartsWith("http://") && !server.StartsWith("https://"))
//			{
//				server = "http://" + server;
//			}
//			client.BaseAddress = new Uri(server);

//			// Basic Authentication header'ını oluşturup ekliyoruz.
//			var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
//			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
//		client.Timeout = TimeSpan.FromMinutes(3);
//		return client;
//		}

//	public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string>? headers = null)
//	{
//		using var client = CreateClient();

//		// Opsiyonel header'ları ekleyelim.
//		if (headers != null)
//		{
//			foreach (var header in headers)
//			{
//				if (!client.DefaultRequestHeaders.Contains(header.Key))
//				{
//					client.DefaultRequestHeaders.Add(header.Key, header.Value);
//				}
//			}
//		}

//		var response = await client.GetAsync(endpoint);
//		response.EnsureSuccessStatusCode();

//		// Eğer T tipi string ise, ham yanıtı döndürelim.
//		if (typeof(T) == typeof(string))
//		{
//			var stringResponse = await response.Content.ReadAsStringAsync();
//			return (T)(object)stringResponse;
//		}
//		else
//		{
//			return await response.Content.ReadFromJsonAsync<T>();
//		}
//	}

//	//public async Task<T> GetAsync<T>(string endpoint)
//	//{
//	//	using var client = CreateClient();
//	//	var response = await client.GetAsync(endpoint);
//	//	response.EnsureSuccessStatusCode();
//	//	return await response.Content.ReadFromJsonAsync<T>();
//	//}

//	public async Task<TResponse> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
//		{
//			using var client = CreateClient();
//			var response = await client.PostAsJsonAsync(endpoint, data);
//			response.EnsureSuccessStatusCode();
//			return await response.Content.ReadFromJsonAsync<TResponse>();
//		}

//		public async Task<TResponse> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
//		{
//			using var client = CreateClient();
//			var response = await client.PutAsJsonAsync(endpoint, data);
//			response.EnsureSuccessStatusCode();
//			return await response.Content.ReadFromJsonAsync<TResponse>();
//		}

//		public async Task<bool> DeleteAsync(string endpoint)
//		{
//			using var client = CreateClient();
//			var response = await client.DeleteAsync(endpoint);
//			return response.IsSuccessStatusCode;
//		}
//	}