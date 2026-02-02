using Application.Interfaces.ConnectionModule.WindchillConnectionModule;
using Domain.Entities;
using Domain.Entities.Connections;
using DotNetEnv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories.ConnectionModule.WindchillConnectionModule;

public class WindchillConnectionRepository : IWindchillConnectionService
{
	private readonly HttpClient _httpClient;

	public WindchillConnectionRepository(HttpClient httpClient)
	{
		_httpClient = httpClient;
	}
	public async Task<bool> CheckWindchillConnectionInformation(WindchillConnectionSettings connectionSettings)
	{
		try
		{
			// Windchill API'sine istek atılacak URL
			var url = $"https://{connectionSettings.WindchillServer}/Windchill/servlet/odata/PTC/GetCSRFToken()";

			var request = new HttpRequestMessage(HttpMethod.Get, url);

			var authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{connectionSettings.WindchillUsername}:{connectionSettings.WindchillPassword}"));
			request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

			var response = await _httpClient.SendAsync(request);

			return response.IsSuccessStatusCode;
		}
		catch
		{
			return false;
		}
	}
    

	public Task<WindchillConnectionSettings> GetConnectionInformation()
	{
		Env.Load();

		string Windchill_Server = Env.GetString("Windchill_Server");
		string Windchill_Username = Env.GetString("Windchill_Username");
		string Windchill_Password = Env.GetString("Windchill_Password");

		var connectionSettings = new WindchillConnectionSettings
		{
			WindchillServer = Windchill_Server,
			WindchillUsername = Windchill_Username,
			WindchillPassword = Windchill_Password,
		};

		return Task.FromResult(connectionSettings);
	}

	public async Task<WindchillConnectionSettings> UpdateConnectionInformation(WindchillConnectionSettings connectionSettings)
	{
		// .env dosyasının yolu
		string envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");

		// Dosyayı asenkron olarak oku
		string[] lines = await System.IO.File.ReadAllLinesAsync(envFile);

		// Dosyadaki satırları güncelle
		for (int i = 0; i < lines.Length; i++)
		{
			if (lines[i].StartsWith("Windchill_Server="))
			{
				lines[i] = $"Windchill_Server={connectionSettings.WindchillServer}";
			}
			else if (lines[i].StartsWith("Windchill_Username="))
			{
				lines[i] = $"Windchill_Username={connectionSettings.WindchillUsername}";
			}
			else if (lines[i].StartsWith("Windchill_Password="))
			{
				lines[i] = $"Windchill_Password={connectionSettings.WindchillPassword}";
			}
		
		}

		// Güncellenmiş satırları asenkron olarak yaz
		await System.IO.File.WriteAllLinesAsync(envFile, lines);

		// Güncellenmiş bağlantı bilgilerini döndür
		return connectionSettings;

	}
}
