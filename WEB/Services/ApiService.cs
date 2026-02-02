using System.Net.Http.Headers;

namespace WEB.Services;

public class ApiService
{
	private readonly HttpClient _httpClient;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
	{
		_httpClient = httpClient;
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<HttpResponseMessage> GetUserAsync()
	{
		var token = _httpContextAccessor.HttpContext.Request.Cookies["JWTToken"];

		if (!string.IsNullOrEmpty(token))
		{
			_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		return await _httpClient.GetAsync("api/users/me");

		//return await _httpClient.GetAsync("api/users/me");
	}
}