using Application.Features.IntegrationSettings.RoleProcessTags.Commands.Create;
using Application.Features.IntegrationSettings.RoleProcessTags.Commands.Delete;
using Application.Features.IntegrationSettings.RoleProcessTags.Commands.Update;
using Application.Features.IntegrationSettings.RoleProcessTags.Queries.GetById;
using Application.Features.IntegrationSettings.RoleProcessTags.Queries.GetList;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers.IntegrationSettings
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleProcessTagsController : BaseController
    {
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var query = new GetRoleProcessTagsQuery();
			var result = await Mediator.Send(query);
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var query = new GetRoleProcessTagByIdQuery { ProcessTagID = id };
			var result = await Mediator.Send(query);
			return Ok(result);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CreateRoleProcessTagCommand command)
		{
			var result = await Mediator.Send(command);
			return Ok(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleProcessTagCommand command)
		{
			command.ProcessTagID = id; // id'yi güncelleme için ata
			var result = await Mediator.Send(command);
			return Ok(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var command = new DeleteRoleProcessTagCommand { ProcessTagID = id };
			var result = await Mediator.Send(command);
			return Ok(result);
		}
	}
}
