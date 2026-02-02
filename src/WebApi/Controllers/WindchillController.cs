using Application.Features.WindchillManagement.Queries.GetWindchillAttributes;
using Application.Features.WindchillManagement.Queries.WtToken.GetList;
using Application.Features.WindchillManagement.Queries.WtUser.GetUsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WindchillController : BaseController
{
	[HttpGet("WindchillToken")]
	public async Task<IActionResult> GetWinddchillToken()
	{
		GetTokenQuery getTokenQuery = new();
		GetTokenItemDto response = await Mediator.Send(getTokenQuery);
		return Ok(response);
	}

	[Authorize]
	[HttpGet("WindchillUsers")]
	public async Task<IActionResult> GetWindchillUsers([FromQuery] string searchTerm)
	{

		var getUsersQuery = new GetUsersQuery { SearchTerm = searchTerm };

		var response = await Mediator.Send(getUsersQuery);

		return Ok(response);
	}

	[HttpGet("WindchillAttributes")]
	public async Task<IActionResult> GetWindchillAttributes()
	{
		var query = new GetWindchillAttributesQuery();
		var attributes = await Mediator.Send(query);
		return Ok(attributes);
	}
}
