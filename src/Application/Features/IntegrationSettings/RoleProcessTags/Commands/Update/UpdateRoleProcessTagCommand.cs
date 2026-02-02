using Application.Interfaces.IntegrationSettings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleProcessTags.Commands.Update;

public class UpdateRoleProcessTagCommand : IRequest<UpdateRoleProcessTagResponse>
{
	public int ProcessTagID { get; set; }
	public string TagName { get; set; }

	public class UpdateRoleProcessTagCommandHandler : IRequestHandler<UpdateRoleProcessTagCommand, UpdateRoleProcessTagResponse>
	{
		private readonly IIntegrationSettingsService _service;

		public UpdateRoleProcessTagCommandHandler(IIntegrationSettingsService service)
		{
			_service = service;
		}

		public async Task<UpdateRoleProcessTagResponse> Handle(UpdateRoleProcessTagCommand request, CancellationToken cancellationToken)
		{
			// Mevcut entity'yi alalım.
			var existingTag = await _service.GetRoleProcessTagByIdAsync(request.ProcessTagID);
			if (existingTag == null)
			{
				return new UpdateRoleProcessTagResponse
				{
					Success = false,
					Message = "Process Tag bulunamadı."
				};
			}

			existingTag.TagName = request.TagName;
			var updatedTag = await _service.UpdateRoleProcessTagAsync(existingTag);
			return new UpdateRoleProcessTagResponse
			{
				Success = true,
				Message = "Process Tag başarıyla güncellendi.",
				ProcessTagID = updatedTag.ProcessTagID
			};
		}
	}
}