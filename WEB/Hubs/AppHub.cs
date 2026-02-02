using WEB.Controllers.Ayarlar;
using WEB.Interfaces;
using Microsoft.AspNetCore.SignalR;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WEB.Hubs;
public class AppHub : Hub
{
	private readonly IApiService _apiService;

	public AppHub(IApiService apiService)
	{
		_apiService = apiService;
	}

	public async Task SendLogUpdate()
	{
		// Bugünün tarihini al
		var date = DateTime.Today;

		// API'den logları çek
		var logResponse = await _apiService.GetAsync<LogResponse>($"api/AuditLogs/by-date?date={date:yyyy-MM-dd}");

		// Logları ters sırala (en yeni log en üstte)
		var sortedLogs = logResponse.Logs.OrderByDescending(l => l.TimeStamp).ToList();

		if (sortedLogs.Any())
		{
			// Yeni logları istemcilere gönder
			await Clients.All.SendAsync("ReceiveLogUpdate", sortedLogs);
		}
	}

	public async Task SendNotification(string message)
	{
		await Clients.All.SendAsync("ReceiveNotification", message);
	}
}
