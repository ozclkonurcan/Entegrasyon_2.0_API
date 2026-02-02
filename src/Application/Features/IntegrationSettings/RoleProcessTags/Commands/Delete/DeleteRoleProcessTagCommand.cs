using Application.Interfaces.IntegrationSettings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleProcessTags.Commands.Delete;

public class DeleteRoleProcessTagCommand : IRequest<DeleteRoleProcessTagResponse>
{
	public int ProcessTagID { get; set; }

	public class DeleteRoleProcessTagCommandHandler : IRequestHandler<DeleteRoleProcessTagCommand, DeleteRoleProcessTagResponse>
	{
		private readonly IIntegrationSettingsService _service;

		public DeleteRoleProcessTagCommandHandler(IIntegrationSettingsService service)
		{
			_service = service;
		}

		public async Task<DeleteRoleProcessTagResponse> Handle(DeleteRoleProcessTagCommand request, CancellationToken cancellationToken)
		{
			var result = await _service.DeleteRoleProcessTagAsync(request.ProcessTagID);
			return new DeleteRoleProcessTagResponse
			{
				Success = result,
				Message = result ? "Process Tag başarıyla silindi." : "Process Tag silinirken hata oluştu."
			};
		}
	}
}