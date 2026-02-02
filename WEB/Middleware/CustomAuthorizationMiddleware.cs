namespace WEB.Middleware;

public class CustomAuthorizationMiddleware
{
	private readonly RequestDelegate _next;

	public CustomAuthorizationMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task Invoke(HttpContext context)
	{
		try
		{
			var token = context.Request.Cookies["JWTToken"];
			if (!string.IsNullOrEmpty(token))
			{
				context.Request.Headers["Authorization"] = "Bearer " + token;
			}

			await _next(context);
		}
		catch (Exception ex)
		{
			// Hata durumunda loglama yapın
			var logger = context.RequestServices.GetRequiredService<ILogger<CustomAuthorizationMiddleware>>();
			logger.LogError(ex, "CustomAuthorizationMiddleware'de hata");

			await _next(context);
		}
	}

	//public async Task Invoke(HttpContext context)
	//{
	//	var token = context.Request.Cookies["JWTToken"];
	//	if (!string.IsNullOrEmpty(token))
	//	{
	//		context.Request.Headers.Add("Authorization", "Bearer " + token);
	//	}

	//	await _next(context);
	//}
}