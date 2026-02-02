using Microsoft.Extensions.Configuration;

namespace WEB.Handlers.HttpHandlers;

public class DynamicApiAddressHandler : DelegatingHandler
{
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IConfiguration _configuration;

	public DynamicApiAddressHandler(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
	{
		_httpContextAccessor = httpContextAccessor;
		_configuration = configuration;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// API adresini appsettings.json'dan al
		var apiUrl = _configuration["DesigntechSettings:httpUrl"];
		var apiPort = _configuration["DesigntechSettings:PortNumber"];
		var fullApiUrl = $"{apiUrl}{apiPort}";

		// API adresini isteğe ekle
		request.RequestUri = new Uri(new Uri(fullApiUrl), request.RequestUri.PathAndQuery);

		return await base.SendAsync(request, cancellationToken);
	}
}
