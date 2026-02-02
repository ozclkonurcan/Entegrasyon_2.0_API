using WEB.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEB.Controllers.Ayarlar
{
	public class SetupController : Controller
	{
		private readonly IApiService _apiService;

		public SetupController(IApiService apiService)
		{
			_apiService = apiService;
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> Index()
		{

			bool isConnectionSuccess = await _apiService.CheckSqlConnectionAsync();
			bool areTablesReady = await _apiService.CheckTablesAsync();

			if (isConnectionSuccess && areTablesReady)
			{
				if (User.Identity.IsAuthenticated)
				{
					return RedirectToAction("Index", "Home");
				}
				else
				{
					return RedirectToAction("", "Auth");
				}
			}
			return View();
		}


	}
}
