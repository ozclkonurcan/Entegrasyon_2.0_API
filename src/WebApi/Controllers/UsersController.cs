using Application.Features.Users.Commands.Create;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : BaseController
{

	[Authorize]
	[HttpGet("info")]
	public IActionResult GetUserInfo()
	{
		var userName = User.FindFirst(ClaimTypes.Name)?.Value;
		var email = User.FindFirst(ClaimTypes.Email)?.Value;
		var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		return Ok(new { UserName = userName, Email = email, UserId = userId });
	}

	[HttpPost]
	public async Task<IActionResult> Add([FromBody] CreateUserCommand createUserCommand)
	{
		try
		{
		CreatedUserResponse result = await Mediator.Send(createUserCommand);
		return Created(uri: "", result);
		}
		catch (Exception ex)
		{

			throw;
		}
	}

}
