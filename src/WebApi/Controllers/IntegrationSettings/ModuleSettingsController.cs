using Application.Features.IntegrationSettings.ModuleSettings.Queries.GetById;
using Application.Features.IntegrationSettings.ModuleSettings.Queries.GetList;
using Application.Features.IntegrationSettings.RoleMappings.Commands.Create;
using Application.Features.IntegrationSettings.RoleMappings.Commands.Delete;
using Application.Features.IntegrationSettings.RoleMappings.Commands.Update;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.IntegrationSettings;

[Route("api/[controller]")]
[ApiController]
public class ModuleSettingsController : BaseController
{
	[HttpGet]
	public async Task<IActionResult> GetAll()
	{
		var query = new GetModuleSettingsQuery();
		var result = await Mediator.Send(query);
		return Ok(result);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> GetById(int id)
	{
		var query = new GetModuleSettingsByIdQuery { Id = id };
		var result = await Mediator.Send(query);
		return Ok(result);
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] CreateModuleSettingsCommand command)
	{
		var response = await Mediator.Send(command);
		return Ok(response);
	}

	[HttpPut("{id}")]
	public async Task<IActionResult> Update(int id, [FromBody] UpdateModuleSettingsCommand command)
	{
		if (id != command.Id)
			return BadRequest("ID hatali.");
		var response = await Mediator.Send(command);
		return Ok(response);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> Delete(int id)
	{
		var command = new DeleteModuleSettingsCommand { Id = id };
		var response = await Mediator.Send(command);
		return Ok(response);
	}
}
