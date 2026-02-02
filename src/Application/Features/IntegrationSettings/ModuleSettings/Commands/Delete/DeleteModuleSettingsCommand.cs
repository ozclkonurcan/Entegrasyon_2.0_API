using Application.Interfaces.IntegrationSettings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleMappings.Commands.Delete;

public class DeleteModuleSettingsCommand : IRequest<DeleteModuleSettingsResponse>
{
	public int Id { get; set; }
	public class DeleteModuleSettingsCommandHandler : IRequestHandler<DeleteModuleSettingsCommand, DeleteModuleSettingsResponse>
	{
		private readonly IIntegrationSettingsService _service;

		public DeleteModuleSettingsCommandHandler(IIntegrationSettingsService service)
		{
			_service = service;
		}
		public async Task<DeleteModuleSettingsResponse> Handle(DeleteModuleSettingsCommand request, CancellationToken cancellationToken)
		{
			var result = await _service.DeleteIntegrationModuleSettingAsync(request.Id);
			return new DeleteModuleSettingsResponse
			{
				Success = result,
				Message = result ? "Module silme basarili" : "Modul silme hatali"
			};
		}
	}
}
