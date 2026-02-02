using Application.Features.IntegrationSettings.ModuleSettings.Commands.Create;
using Application.Features.IntegrationSettings.ModuleSettings.Commands.Delete;
using Application.Features.IntegrationSettings.ModuleSettings.Commands.Update;
using Application.Features.IntegrationSettings.RoleMappings.Queries.GetById;
using Application.Features.IntegrationSettings.RoleMappings.Queries.GetList;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.IntegrationSettings;

[Route("api/[controller]")]
[ApiController]
public class RoleMappingsController : BaseController
{
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var query = new GetRoleMappingsQuery();
		var result = await Mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(int id)
	{
		var query = new GetRoleMappingByIdQuery { Id = id };
		var result = await Mediator.Send(query);
		return Ok(result);
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateRoleMappingCommand command)
	{
		var response = await Mediator.Send(command);
		return Ok(response);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleMappingCommand command)
	{
		if (id != command.Id)
			return BadRequest("ID hatali.");
		var response = await Mediator.Send(command);
		return Ok(response);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(int id)
	{
		
		var command = new DeleteRoleMappingCommand { Id = id };
		var response = await Mediator.Send(command);
		return Ok(response);
		
	}



}
