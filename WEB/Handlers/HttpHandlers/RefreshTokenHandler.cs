
using WEB.Interfaces;
using WEB.Models;
using System.Net;
using System.Net.Http.Headers;

public class RefreshTokenHandler : DelegatingHandler
{
	private readonly IApiService _apiService;
	private readonly IHttpContextAccessor _httpContextAccessor;

	public RefreshTokenHandler(IApiService apiService, IHttpContextAccessor httpContextAccessor)
	{
		_apiService = apiService;
		_httpContextAccessor = httpContextAccessor;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// İlk istek gönderimi
		var response = await base.SendAsync(request, cancellationToken);

		// Eğer 401 (Unauthorized) yanıtı alındıysa token yenilemeyi dene
		if (response.StatusCode == HttpStatusCode.Unauthorized)
		{
			var httpContext = _httpContextAccessor.HttpContext;
			var refreshToken = httpContext.Request.Cookies["RefreshToken"];

			if (!string.IsNullOrEmpty(refreshToken))
			{
				var refreshRequest = new RefreshTokenRequest
				{
					RefreshToken = refreshToken,
					IpAddress = httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1"
				};

				try
				{
					// Refresh token API çağrısı
					var tokenResponse = await _apiService.RefreshTokenAsync(refreshRequest);

					if (tokenResponse != null)
					{
						// Yeni token'ı cookie'lere kaydet
						httpContext.Response.Cookies.Append("JWTToken", tokenResponse.Token, new CookieOptions
						{
							HttpOnly = true,
							Secure = false,
							SameSite = SameSiteMode.Strict,
							Expires = DateTime.UtcNow.AddHours(1)
						});
						httpContext.Response.Cookies.Append("RefreshToken", tokenResponse.RefreshToken, new CookieOptions
						{
							HttpOnly = true,
							Secure = false,
							SameSite = SameSiteMode.Strict,
							Expires = DateTime.UtcNow.AddDays(30)
						});

						// Orijinal isteğin authorization header’ını güncelle
						request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokenResponse.Token);

						// İsteği yeniden gönder
						response = await base.SendAsync(request, cancellationToken);
					}
				}
				catch (Exception ex)
				{
					// Refresh token işlemi başarısızsa, burada loglama yapabilir veya login sayfasına yönlendirme kararını verebilirsiniz.
				}
			}
		}
		return response;
	}
}
