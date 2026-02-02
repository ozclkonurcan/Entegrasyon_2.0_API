using Application.Interfaces.IntegrationSettings;
using Domain.Entities.IntegrationSettings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleMappings.Commands.Create;

public class CreateModuleSettingsCommand : IRequest<CreateModuleSettingsResponse>
{
	public string SettingsName { get; set; }
	public byte SettingsValue { get; set; }
	public class CreateModuleSettingsCommandHandler : IRequestHandler<CreateModuleSettingsCommand, CreateModuleSettingsResponse>
	{
		private readonly IIntegrationSettingsService _service;

		public CreateModuleSettingsCommandHandler(IIntegrationSettingsService service)
		{
			_service = service;
		}

		public async Task<CreateModuleSettingsResponse> Handle(CreateModuleSettingsCommand request, CancellationToken cancellationToken)
		{
			var newSetting = new IntegrationModuleSettings
			{
				SettingsName = request.SettingsName,
				SettingsValue = request.SettingsValue
			};

			var createdSetting = await _service.CreateIntegrationModuleSettingAsync(newSetting);
			return new CreateModuleSettingsResponse
			{
				Success = true,
				Message = "Modul ayarlari olusturuldu",
				Id = createdSetting.Id
			};
		}
	}
}
