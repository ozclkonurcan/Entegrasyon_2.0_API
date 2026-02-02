using WEB.Models; // Bu using ifadesini ekleyin
using WEB.Interfaces;
using System.Net;
using System.Net.Http.Headers;

namespace WEB.Handlers
{
	public class TokenRefreshHandler : DelegatingHandler
	{
		private readonly IApiService _apiService;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public TokenRefreshHandler(IApiService apiService, IHttpContextAccessor httpContextAccessor)
		{
			_apiService = apiService;
			_httpContextAccessor = httpContextAccessor;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var response = await base.SendAsync(request, cancellationToken);

			// 401 durumunda refresh token isteği yapalım
			if (response.StatusCode == HttpStatusCode.Unauthorized)
			{
				var refreshToken = _httpContextAccessor.HttpContext.Request.Cookies["RefreshToken"];

				if (!string.IsNullOrEmpty(refreshToken))
				{
					// API’ye refresh-token isteği gönderin
					var newTokenResponse = await _apiService.RefreshTokenAsync(
						new RefreshTokenRequest
						{
							RefreshToken = refreshToken,
							IpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString()
						});

					if (newTokenResponse != null)
					{
						// Yeni access token bilgisi alındıysa, request header’ını güncelleyin
						request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newTokenResponse.Token);

						// Yeni token’ları cookie’ye koyun
						_httpContextAccessor.HttpContext.Response.Cookies.Append("JWTToken", newTokenResponse.Token, new CookieOptions { HttpOnly = true, Expires = DateTime.UtcNow.AddHours(1) });
						_httpContextAccessor.HttpContext.Response.Cookies.Append("RefreshToken", newTokenResponse.RefreshToken, new CookieOptions { HttpOnly = true, Expires = DateTime.UtcNow.AddDays(30) });

						// Request’i yeniden gönderin
						response = await base.SendAsync(request, cancellationToken);
					}
				}
			}

			return response;
		}
	}
}