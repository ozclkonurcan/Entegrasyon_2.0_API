using MediatR;
using System.IdentityModel.Tokens.Jwt;

namespace WEB.Pipelines.TokenControl;

public class TokenValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : IRequest<TResponse>,ITokenValidationBehavior
{
	private readonly IHttpContextAccessor _httpContextAccessor;

	public TokenValidationBehavior(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
	{
		var token = _httpContextAccessor.HttpContext.Session.GetString("JWTToken");

		if (string.IsNullOrEmpty(token) || !IsTokenValid(token))
		{
			// Token geçersizse veya yoksa, oturum açma sayfasına yönlendir
			_httpContextAccessor.HttpContext.Session.Remove("JWTToken");
			_httpContextAccessor.HttpContext.Response.Redirect("/Auth/Login");
			return default; // İşlemi sonlandır
		}

		// Token geçerliyse, bir sonraki işleme devam et
		return await next();
	}

	private bool IsTokenValid(string token)
	{
		try
		{
			var handler = new JwtSecurityTokenHandler();
			var jwtToken = handler.ReadJwtToken(token);

			// Token'in süresi dolmuşsa geçersiz kabul et
			if (jwtToken.ValidTo < DateTime.UtcNow)
			{
				return false;
			}

			return true;
		}
		catch
		{
			return false;
		}
	}
}