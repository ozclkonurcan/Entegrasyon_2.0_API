using Application.Interfaces.IntegrationSettings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleMappings.Commands.Update;

public class UpdateModuleSettingsCommand : IRequest<UpdateModuleSettingsResponse>
{
	public int Id { get; set; }
	public string SettingsName { get; set; }
	public byte SettingsValue { get; set; }
	public class UpdateModuleSettingsCommandHandler : IRequestHandler<UpdateModuleSettingsCommand, UpdateModuleSettingsResponse>
	{
		private readonly IIntegrationSettingsService _service;

		public UpdateModuleSettingsCommandHandler(IIntegrationSettingsService service)
		{
			_service = service;
		}
		public async Task<UpdateModuleSettingsResponse> Handle(UpdateModuleSettingsCommand request, CancellationToken cancellationToken)
		{
			var settingToUpdate = new Domain.Entities.IntegrationSettings.IntegrationModuleSettings
			{
				Id = request.Id,
				SettingsName = request.SettingsName,
				SettingsValue = request.SettingsValue
			};

			var updatedSetting = await _service.UpdateIntegrationModuleSettingAsync(settingToUpdate);
			return new UpdateModuleSettingsResponse
			{
				Success = true,
				Message = "Modul ayarlari guncellendi"
			};
		}
	}
}
