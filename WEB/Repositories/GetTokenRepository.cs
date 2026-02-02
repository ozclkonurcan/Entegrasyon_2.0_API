using WEB.Interfaces;

namespace WEB.Repositories;

public class GetTokenRepository : IGetTokenService
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public GetTokenRepository(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public Task<string> GetTokenAsync()
	{
		var token = _httpContextAccessor.HttpContext?.Request.Cookies["JWTToken"];
		return Task.FromResult(token);
	}
}
