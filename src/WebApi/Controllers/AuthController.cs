using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.Commands.Logout;
using Application.Features.Auth.Commands.RefreshToken;
using Infrastructure.BackgroundServices;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Security.Entities;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : BaseController
{

	[HttpPost("login")]
	public async Task<IActionResult> Login([FromBody] LoginCommand command)
	{
		command.IpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
		var response = await Mediator.Send(command);
		return Ok(response);
	}

	//[HttpPost("login")]
	//public async Task<IActionResult> Login([FromBody] LoginCommand command)
	//{
	//	//IntegrationBackgroundService(); login işlemi ypaıldıktan sonra tetik çalıştırılabilir


	//	var response = await Mediator.Send(command);
	//	return Ok(response);
	//}

	[HttpPost("logout")]
	public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
	{
		//IntegrationBackgroundService(); logout işlemi ypaıldıktan sonra tetik kapattırılabilir
		await Mediator.Send(command);
		return Ok(new { message = "Çikis basarili." });
	}


	[HttpGet("validate-token")]
	public IActionResult ValidateToken()
	{
		// Token'ın geçerliliği otomatik olarak JWT middleware tarafından kontrol edilir
		return Ok(new { isValid = true });
	}


	[HttpPost("refresh-token")]
	public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand request)
	{
		// IP bilgisini burada da set edelim (isteğe bağlı, aynı zamanda client da gönderebilir)
		request.IpAddress = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
		var response = await Mediator.Send(new RefreshTokenCommand
		{
			RefreshToken = request.RefreshToken,
			IpAddress = request.IpAddress
		});
		return Ok(response);
	}

}
