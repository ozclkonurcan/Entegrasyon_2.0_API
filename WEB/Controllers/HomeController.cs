using WEB.Hubs;
using WEB.Interfaces;
using WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;

namespace WEB.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IApiService _apiService;
		private readonly IHubContext<WTPartHub> _hubContext;

		public HomeController(ILogger<HomeController> logger, IApiService apiService, IHubContext<WTPartHub> hubContext)
		{
			_logger = logger;
			_apiService = apiService;
			_hubContext = hubContext;
		}

		public async Task<IActionResult> Index()
		{
			try
			{
				var viewModel = await GetIntegrationSummaryAsync();
				return View(viewModel);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Index sayfası yüklenirken hata oluştu");

				// Hata durumunda boş bir model ile devam et
				var emptyModel = new WTPartIntegrationIndexViewModel
				{
					ReleasedSentCount = 0,
					CancelledSentCount = 0,
					ReleasedNotSentCount = 0,
					CancelledNotSentCount = 0,
					WtpartAlternateCount = 0,
					WtpartAlternateSentCount = 0,
					WtpartAlternateRemovedCount = 0,
					WtpartAlternateRemovedSentCount = 0,
					WtpartAlternateRemovedErrorCount = 0,
					WtpartAlternateErrorCount = 0,
					CancelledErrorCount = 0,
					ReleasedErrorCount = 0,
				};

				return View(emptyModel);
			}

		}



		private async Task<WTPartIntegrationIndexViewModel> GetIntegrationSummaryAsync()
		{
			try
			{
				// Sadece sentdatas API çağrısını yap
				var sentData = await _apiService.GetAsync<List<WTPartViewModel>>("api/WTParts/sentdatas");
				var pendingData = await _apiService.GetAsync<List<WTPartViewModel>>("api/WTParts/getlistall");
				var errorData = await _apiService.GetAsync<List<WTPartError>>("api/WTParts/errordatas");
				var sentDataAlternate = await _apiService.GetAsync<List<WTPartAlternateLink>>("api/WTParts/sentdatasalternate");
				var errorDataAlternate = await _apiService.GetAsync<List<WTPartAlternateLink>>("api/WTParts/errordatasalternate");
				var sentDataAlternateRemoved = await _apiService.GetAsync<List<WTPartAlternateLinkRemoved>>("api/WTParts/sentdatasalternateremoved");
				var errorDataAlternateRemoved = await _apiService.GetAsync<List<WTPartAlternateLinkRemoved>>("api/WTParts/errordatasalternateremoved");// bunda bi bozuklu oldu bakacaz
				var pendingAlternateData = await _apiService.GetAsync<List<WTPartAlternateLink>>("api/WTParts/getlistallalternatelink");
				var pendingAlternateRemovedData = await _apiService.GetAsync<List<WTPartAlternateLinkRemoved>>("api/WTParts/getlistallalternatelinkRemoved");
				// pendingData için boş liste kullan
				//var pendingData = new List<WTPartViewModel>();

				// Null kontrolü ile güvenli filtreleme
				var releasedSent = sentData?.Where(x => x?.ParcaState == "RELEASED").ToList() ?? new List<WTPartViewModel>();
				var cancelledSent = sentData?.Where(x => x?.ParcaState == "CANCELLED").ToList() ?? new List<WTPartViewModel>();
				var releasedPending = pendingData?.Where(x => x.ParcaState == "RELEASED").ToList() ?? new List<WTPartViewModel>();
				var cancelledPending = pendingData?.Where(x => x.ParcaState == "CANCELLED").ToList() ?? new List<WTPartViewModel>();
				var releasedError = errorData?.Where(x => x.ParcaState == "RELEASED").ToList() ?? new List<WTPartError>();
				var cancelledError = errorData?.Where(x => x.ParcaState == "CANCELLED").ToList() ?? new List<WTPartError>();
				var alternatePending = pendingAlternateData ?? new List<WTPartAlternateLink>();
				var alternateRemovedPending = pendingAlternateRemovedData ?? new List<WTPartAlternateLinkRemoved>();
				var alternateSent = sentDataAlternate ?? new List<WTPartAlternateLink>();
				var alternateError = errorDataAlternate ?? new List<WTPartAlternateLink>();
				var alternateRemovedSent = sentDataAlternateRemoved ?? new List<WTPartAlternateLinkRemoved>();
				var alternateRemovedError = errorDataAlternateRemoved ?? new List<WTPartAlternateLinkRemoved>(); // bilerek null yaptım api de orada bir sıkıntı var sanırım sonra bakacam
				//var releasedPending = new List<WTPartViewModel>(); // Boş liste
				//var cancelledPending = new List<WTPartViewModel>(); // Boş liste

				return new WTPartIntegrationIndexViewModel
				{
					ReleasedSentCount = releasedSent.Count,
					CancelledSentCount = cancelledSent.Count,
					ReleasedNotSentCount = releasedPending?.Count ?? 0,
					CancelledNotSentCount = cancelledPending?.Count ?? 0,
					ReleasedErrorCount = releasedError?.Count ?? 0,
					CancelledErrorCount = cancelledError?.Count ?? 0,
					WtpartAlternateCount = alternatePending?.Count ?? 0,
					WtpartAlternateSentCount = alternateSent?.Count ?? 0,
					WtpartAlternateErrorCount = alternateError?.Count ?? 0,
					WtpartAlternateRemovedCount = alternateRemovedPending?.Count ?? 0,
					WtpartAlternateRemovedSentCount = alternateRemovedSent?.Count ?? 0,
					WtpartAlternateRemovedErrorCount = alternateRemovedError?.Count ?? 0,
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Entegrasyon özeti alınırken hata oluştu");

				// Hata durumunda boş model döndür
				return new WTPartIntegrationIndexViewModel
				{
					ReleasedSentCount = 0,
					CancelledSentCount = 0,
					ReleasedNotSentCount = 0,
					CancelledNotSentCount = 0,
					WtpartAlternateCount = 0,
					WtpartAlternateSentCount = 0,
					WtpartAlternateRemovedCount = 0,
					WtpartAlternateRemovedSentCount = 0,
					WtpartAlternateRemovedErrorCount = 0,
					WtpartAlternateErrorCount = 0,
					CancelledErrorCount = 0,
					ReleasedErrorCount = 0,
				};
			}
		}

	

		// SignalR üzerinden tüm bağlı istemcilerin canlı güncellenebilmesi için Push bildirimi gönderen endpoint.
		[HttpPost]
		public async Task<IActionResult> PushIntegrationSummary()
		{
			var viewModel = await GetIntegrationSummaryAsync();

			// WTPart güncellemeleri
			await _hubContext.Clients.All.SendAsync(
				"ReceiveWTPartUpdates",
				viewModel.ReleasedNotSentCount,
				viewModel.ReleasedSentCount,
				viewModel.CancelledNotSentCount,
				viewModel.CancelledSentCount,
				viewModel.ReleasedErrorCount,
				viewModel.CancelledErrorCount);

			// AlternateLink güncellemeleri - ayrı bir olay olarak gönder
			await _hubContext.Clients.All.SendAsync(
				"ReceiveAlternateLinkUpdates",
				viewModel.WtpartAlternateCount,
				viewModel.WtpartAlternateSentCount,
				viewModel.WtpartAlternateRemovedCount,
				viewModel.WtpartAlternateRemovedSentCount,
				viewModel.WtpartAlternateErrorCount,
				viewModel.WtpartAlternateRemovedErrorCount); // removedTotalCount - şu an için 0 kullanıyoruz

			return Ok("Güncelleme bildirimi gönderildi.");
		}

		

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel
			{
				RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
			});
		}
	}
}
