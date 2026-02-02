

using System.Net.Http.Headers;

namespace WEB.Handlers.HttpHandlers;

public class AuthHeaderHandler : DelegatingHandler
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public AuthHeaderHandler(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// Cookie'den token'ı al
		var token = _httpContextAccessor.HttpContext.Request.Cookies["JWTToken"];

		if (!string.IsNullOrEmpty(token))
		{
			// Token'ı Authorization başlığına ekle
			request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}

		return await base.SendAsync(request, cancellationToken);
	}
}
