using WEB.Interfaces;
using WEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WEB.Controllers.Ayarlar;
[Authorize]
public class EntegrasyonRoleController : Controller
{
	private readonly IApiService _apiService;

	public EntegrasyonRoleController(IApiService apiService)
	{
		_apiService = apiService;
	}

	// Rol listesini getirip view'e gönderir.
	public async Task<IActionResult> Index()
	{
		var roles = await _apiService.GetAsync<List<RoleMappingViewModel>>("api/RoleMappings");
		return View(roles);
	}

	[HttpGet]
	public async Task<IActionResult> GetWindchillAttributes()
	{
		// Dış API'den Windchill attribute listesini alıyoruz.
		var attributes = await _apiService.GetAsync<List<string>>("api/Windchill/WindchillAttributes");
		return Json(attributes);
	}

	// Belirli bir rolü ID ile getirir.
	[HttpGet]
	public async Task<IActionResult> GetRoleById(int id)
	{
		var role = await _apiService.GetAsync<RoleMappingViewModel>($"api/RoleMappings/{id}");
		if (role == null)
			return NotFound();
		return Json(role);
	}

	[HttpGet]
	public async Task<IActionResult> GetProcessTags()
	{
		var tags = await _apiService.GetAsync<List<RoleProcessTagDto>>("api/RoleProcessTags");
		return Json(tags);
	}

	[HttpPost]
	public async Task<IActionResult> CreateRole([FromBody] RoleMappingViewModel model)
	{
		if (ModelState.IsValid)
		{
			await _apiService.PostAsync("api/RoleMappings", model);
			return Ok(new { success = true, message = "Rol başarıyla eklendi." });
		}
		var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
		return BadRequest(new { success = false, errors });
	}

	[HttpPost]
	public async Task<IActionResult> UpdateRole([FromBody] RoleMappingViewModel model)
	{
		if (ModelState.IsValid)
		{
			await _apiService.PutAsync<RoleMappingViewModel, RoleMappingViewModel>($"api/RoleMappings/{model.Id}", model);
			return Ok(new { success = true, message = "Rol başarıyla güncellendi." });
		}
		return BadRequest(ModelState);
	}

	[HttpPost]
	public async Task<IActionResult> ToggleRoleActive(int id, bool isActive)
	{
		// Toggle için full model update: önce mevcut rolü getirip, IsActive güncelleniyor.
		var role = await _apiService.GetAsync<RoleMappingViewModel>($"api/RoleMappings/{id}");
		if (role == null)
			return NotFound();
		role.IsActive = isActive;
		var updatedRole = await _apiService.PutAsync<RoleMappingViewModel, RoleMappingViewModel>($"api/RoleMappings/{id}", role);
		return Ok(new { success = updatedRole != null, isActive = updatedRole?.IsActive });
	}

	[HttpPost]
	public async Task<IActionResult> DeleteRole(int id)
	{
		var result = await _apiService.DeleteAsync($"api/RoleMappings/{id}");
		return Ok(new { success = result });
	}
}