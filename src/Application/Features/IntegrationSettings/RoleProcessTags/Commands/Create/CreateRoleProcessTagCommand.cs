using Application.Interfaces.IntegrationSettings;
using Domain.Entities.IntegrationSettings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleProcessTags.Commands.Create;

public class CreateRoleProcessTagCommand : IRequest<CreateRoleProcessTagResponse>
{
	public string TagName { get; set; }

	public class CreateRoleProcessTagCommandHandler : IRequestHandler<CreateRoleProcessTagCommand, CreateRoleProcessTagResponse>
	{
		private readonly IIntegrationSettingsService _service;

		public CreateRoleProcessTagCommandHandler(IIntegrationSettingsService service)
		{
			_service = service;
		}

		public async Task<CreateRoleProcessTagResponse> Handle(CreateRoleProcessTagCommand request, CancellationToken cancellationToken)
		{
			var newTag = new RoleProcessTag
			{
				TagName = request.TagName
			};

			var createdTag = await _service.CreateRoleProcessTagAsync(newTag);
			return new CreateRoleProcessTagResponse
			{
				Success = true,
				Message = "Process Tag başarıyla eklendi.",
				ProcessTagID = createdTag.ProcessTagID
			};
		}
	}
}
