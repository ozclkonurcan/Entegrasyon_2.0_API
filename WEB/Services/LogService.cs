using WEB.Controllers.Ayarlar;
using WEB.Hubs;
using WEB.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace WEB.Services;

public class LogService
{
	private readonly IApiService _apiService;
	private readonly IHubContext<AppHub> _hubContext;

	public LogService(IApiService apiService, IHubContext<AppHub> hubContext)
	{
		_apiService = apiService;
		_hubContext = hubContext;
	}

	public async Task AddLogAsync(LogDto log)
	{
		try
		{
			// Log ekleme işlemleri...
			var response = await _apiService.PostAsync<LogDto>("api/AuditLogs", log);
			await _hubContext.Clients.All.SendAsync("SendLogUpdate");
			
		}
		catch (Exception ex)
		{
			throw new Exception("Log eklenirken bir hata oluştu: " + ex.Message);
		}
	}
}