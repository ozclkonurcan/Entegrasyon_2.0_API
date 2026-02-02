using WEB.Hubs;
using WEB.Interfaces;
using WEB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace WEB.Controllers.EntegrasyonModule;

public class WTPartModuleController : Controller
{
	private readonly IApiService _apiService;
	private readonly IHubContext<WTPartHub> _hubContext;
	public WTPartModuleController(IApiService apiService, IHubContext<WTPartHub> hubContext)
	{
		_apiService = apiService;
		_hubContext = hubContext;
	}

	// Sayfa açıldığında veriler model olarak gönderilir.
	//public async Task<IActionResult> Index()
	//{
	//	var data = await _apiService.GetAsync<List<WTPartViewModel>>("api/WTParts/sentdatas");
	//	// LogDate'e göre azalan sırada sıralıyoruz (son eklenen en üstte)
	//	data = data.OrderByDescending(x => x.LogDate).ToList();
	//	return View(data);
	//}

	public async Task<IActionResult> Index()
	{
		// İlk yüklemede view modeli oluşturuyoruz.
		var viewModel = await GetIntegrationSummaryAsync();
		return View(viewModel);
	}

	// Canlı güncelleme için JSON döndüren action.
	[HttpGet]
	public async Task<IActionResult> GetIntegrationSummary()
	{
		var viewModel = await GetIntegrationSummaryAsync();
		return Json(viewModel);
	}

	// API'den verileri çekip filtreleyerek view model oluşturan metod.
	private async Task<WTPartIntegrationIndexViewModel> GetIntegrationSummaryAsync()
	{
		// API'den gönderilmiş verileri ve bekleyen verileri çekiyoruz.
		var sentData = await _apiService.GetAsync<List<WTPartViewModel>>("api/WTParts/sentdatas");
		var pendingData = await _apiService.GetAsync<List<WTPartViewModel>>("api/WTParts/getlistall");

		// RELEASED ve CANCELLED durumlarına göre filtreleme.
		var releasedSent = sentData?.Where(x => x.ParcaState == "RELEASED").ToList() ?? new List<WTPartViewModel>();
		var cancelledSent = sentData?.Where(x => x.ParcaState == "CANCELLED").ToList() ?? new List<WTPartViewModel>();
		var releasedPending = pendingData?.Where(x => x.ParcaState == "RELEASED").ToList() ?? new List<WTPartViewModel>();
		var cancelledPending = pendingData?.Where(x => x.ParcaState == "CANCELLED").ToList() ?? new List<WTPartViewModel>();

		return new WTPartIntegrationIndexViewModel
		{
			ReleasedSentCount = releasedSent.Count,
			CancelledSentCount = cancelledSent.Count,
			ReleasedNotSentCount = releasedPending.Count,
			CancelledNotSentCount = cancelledPending.Count
		};
	}

	// Yeni: Push bildirimi tetikleyen endpoint.
	[HttpPost]
	public async Task<IActionResult> PushIntegrationSummary()
	{
		var viewModel = await GetIntegrationSummaryAsync();

		// SignalR üzerinden tüm bağlı istemcilere veri gönderiyoruz.
		await _hubContext.Clients.All.SendAsync(
			"ReceiveWTPartUpdates",
			viewModel.ReleasedNotSentCount,
			viewModel.ReleasedSentCount,
			viewModel.CancelledNotSentCount,
			viewModel.CancelledSentCount);

		return Ok("Push bildirimi gönderildi.");
	}

	[HttpGet]
	public async Task<IActionResult> GetProcessTags()
	{
		var tags = await _apiService.GetAsync<List<RoleProcessTagDto>>("api/RoleProcessTags");
		return Json(tags);
	}

	// AJAX istekleri için JSON formatında güncel veriyi döndürür.
	[HttpGet]
	public async Task<IActionResult> RefreshData()
	{
		var data = await _apiService.GetAsync<List<WTPartViewModel>>("api/WTParts/sentdatas");
		// LogDate'e göre azalan sırada sıralama yapıyoruz
		data = data.OrderByDescending(x => x.LogDate).ToList();
		return Json(data);
	}


	public async Task<IActionResult> FilteredIndex(string filterType = "daily", DateTime? startDate = null, DateTime? endDate = null, string searchText = "")
	{
		// API endpoint URL'si; örneğin:
		// API'niz filtre parametrelerini query string olarak alıyorsa:
		string apiUrl = "api/WTParts/filtered";
		// Parametreleri URL'e ekleyebilirsiniz (kendi IApiService implementasyonunuza göre)
		var queryParams = $"?filterType={filterType}&startDate={startDate?.ToString("o")}&endDate={endDate?.ToString("o")}&searchText={Uri.EscapeDataString(searchText)}";

		var data = await _apiService.GetAsync<List<WTPartViewModel>>(apiUrl + queryParams);
		return View("Index", data);
	}
}
