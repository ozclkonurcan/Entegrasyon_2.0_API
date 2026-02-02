using Application.Interfaces.IntegrationSettings;
using Domain.Entities.IntegrationSettings;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.ModuleSettings.Commands.Create;
public class CreateRoleMappingEndpointDto
{
	public string TargetApi { get; set; }
	public string Endpoint { get; set; }
	public bool IsActive { get; set; }
}

public class CreateRoleMappingCommand : IRequest<CreateRoleMappingResponse>
{
	public string RoleName { get; set; }
	public int ProcessTagID { get; set; }
	public string SourceApi { get; set; }
	//public string TargetApis { get; set; }
	public bool IsActive { get; set; }
	public List<CreateRoleMappingEndpointDto> Endpoints { get; set; } = new List<CreateRoleMappingEndpointDto>();
	public List<RoleAttributeViewModel> WindchillAttributes { get; set; } = new List<RoleAttributeViewModel>();

	public class CreateRoleMappingCommandHandler : IRequestHandler<CreateRoleMappingCommand, CreateRoleMappingResponse>
	{

		private readonly IIntegrationSettingsService _service;

		public CreateRoleMappingCommandHandler(IIntegrationSettingsService service)
		{
			_service = service;
		}

		public async Task<CreateRoleMappingResponse> Handle(CreateRoleMappingCommand request, CancellationToken cancellationToken)
		{
			var newMapping = new RoleMapping
			{
				RoleName = request.RoleName,
				ProcessTagID = request.ProcessTagID,
				SourceApi = request.SourceApi,
				IsActive = request.IsActive
				//TargetApis = request.TargetApis,
			};

			if (request.Endpoints != null && request.Endpoints.Count > 0)
			{
				foreach (var epDto in request.Endpoints)
				{
					newMapping.Endpoints.Add(new RoleMappingEndpoint
					{
						TargetApi = epDto.TargetApi,
						Endpoint = epDto.Endpoint,
						IsActive = epDto.IsActive
					});
				}
			}

			if (request.WindchillAttributes != null && request.WindchillAttributes.Any())
			{
				foreach (var attrName in request.WindchillAttributes)
				{
					newMapping.WindchillAttributes.Add(new RoleMappingAttribute
					{
						AttributeName = attrName.AttributeName,
						IsSelected = true 
					});
				}
			}

			// Repository üzerinden oluşturma işlemi gerçekleştirilir.
			var createdMapping = await _service.CreateRoleMappingAsync(newMapping);
			return new CreateRoleMappingResponse
			{
				Success = true,
				Message = "Rol ekleme basarili.",
				Id = createdMapping.Id
			};

		}
	}
}
