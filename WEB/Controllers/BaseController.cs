using WEB.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace WEB.Controllers;

public class BaseController : Controller
{

	private readonly IConfiguration _configuration;

	public BaseController(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	protected string GetBaseUrl()
	{
		var httpUrl = _configuration["DesigntechSettings:httpUrl"];
		var portNumber = _configuration["DesigntechSettings:PortNumber"];
		return $"{httpUrl}{portNumber}";
	}

	protected HttpClient GetHttpClient()
	{
		var client = new HttpClient();
		var token = HttpContext.Request.Cookies["JWTToken"];
		if (!string.IsNullOrEmpty(token))
		{
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
		}
		client.BaseAddress = new Uri(GetBaseUrl()); // API adresi dinamik olarak alınıyor
		return client;
	}
	//protected HttpClient GetHttpClient()
	//{
	//	var client = new HttpClient();
	//	var token = HttpContext.Request.Cookies["JWTToken"];
	//	if (!string.IsNullOrEmpty(token))
	//	{
	//		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
	//	}
	//	client.BaseAddress = new Uri("https://localhost:7122/"); // API adresi
	//	return client;
	//}
}