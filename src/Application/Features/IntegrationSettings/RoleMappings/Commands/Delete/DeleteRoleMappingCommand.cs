using Application.Interfaces.IntegrationSettings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.ModuleSettings.Commands.Delete;

public class DeleteRoleMappingCommand : IRequest<DeleteRoleMappingResponse>
{
	public int Id { get; set; }
	public class DeleteRoleMappingCommandHandler : IRequestHandler<DeleteRoleMappingCommand, DeleteRoleMappingResponse>
	{
		private readonly IIntegrationSettingsService _service;

		public DeleteRoleMappingCommandHandler(IIntegrationSettingsService service)
		{
			_service = service;
		}

		public async Task<DeleteRoleMappingResponse> Handle(DeleteRoleMappingCommand request, CancellationToken cancellationToken)
		{
			var result = await _service.DeleteRoleMappingAsync(request.Id);
			return new DeleteRoleMappingResponse
			{
				Success = result,
				Message = result ? "Rol Silme basarili" : "Rol Silme hatali."
			};
		}
	}
}
