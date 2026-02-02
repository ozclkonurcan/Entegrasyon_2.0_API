using Application.Features.MailService.Commands.SaveMailSettings;
using Application.Features.MailService.Commands.TestMailConnection;
using Application.Features.MailService.Queries.GetMailSettings;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Produces("application/json")]
	[Tags("Mail Service")]
	public class MailServiceController : ControllerBase
	{
		private readonly IMediator _mediator;

		public MailServiceController(IMediator mediator)
		{
			_mediator = mediator;
		}


		[HttpGet]
		[ProducesResponseType(typeof(GetMailSettingsDto), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(GetMailSettingsDto), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<GetMailSettingsDto>> GetMailSettings()
		{
			var query = new GetMailSettingsQuery();
			var result = await _mediator.Send(query);

			if (!result.Success)
			{
				return NotFound(result);
			}

			return Ok(result);
		}


		[HttpPost]
		[ProducesResponseType(typeof(SaveMailSettingsResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(SaveMailSettingsResponse), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<SaveMailSettingsResponse>> SaveMailSettings([FromBody] SaveMailSettingsCommand command)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _mediator.Send(command);

			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}


		[HttpPut]
		[ProducesResponseType(typeof(SaveMailSettingsResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(SaveMailSettingsResponse), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<SaveMailSettingsResponse>> UpdateMailSettings([FromBody] SaveMailSettingsCommand command)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _mediator.Send(command);

			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}

	
		[HttpPost("test-connection")]
		[ProducesResponseType(typeof(TestMailConnectionResponse), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(TestMailConnectionResponse), StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<TestMailConnectionResponse>> TestMailConnection([FromBody] TestMailConnectionCommand command)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var result = await _mediator.Send(command);

			if (!result.Success)
			{
				return BadRequest(result);
			}

			return Ok(result);
		}
	}
}