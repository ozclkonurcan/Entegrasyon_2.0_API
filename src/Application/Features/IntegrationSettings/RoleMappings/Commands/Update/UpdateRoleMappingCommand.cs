using Application.Features.IntegrationSettings.ModuleSettings.Commands.Create;
using Application.Interfaces.IntegrationSettings;
using Domain.Entities.IntegrationSettings;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.ModuleSettings.Commands.Update
{
	public class UpdateRoleMappingCommand : IRequest<UpdateRoleMappingResponse>
	{
		public int Id { get; set; }
		public string RoleName { get; set; }
		public int ProcessTagID { get; set; }
		public string SourceApi { get; set; }
		//public string TargetApis { get; set; } // Artık Endpoints kullanıyoruz.
		public bool IsActive { get; set; }
		public List<CreateRoleMappingEndpointDto> Endpoints { get; set; } = new List<CreateRoleMappingEndpointDto>();
		public List<RoleAttributeViewModel> WindchillAttributes { get; set; } = new List<RoleAttributeViewModel>();


		public class UpdateRoleMappingCommandHandler : IRequestHandler<UpdateRoleMappingCommand, UpdateRoleMappingResponse>
		{
			private readonly IIntegrationSettingsService _service;

			public UpdateRoleMappingCommandHandler(IIntegrationSettingsService service)
			{
				_service = service;
			}

			public async Task<UpdateRoleMappingResponse> Handle(UpdateRoleMappingCommand request, CancellationToken cancellationToken)
			{
				// Öncelikle mevcut entity'yi veritabanından çekin.
				var mappingToUpdate = await _service.GetRoleMappingByIdAsync(request.Id);
				if (mappingToUpdate == null)
				{
					return new UpdateRoleMappingResponse
					{
						Success = false,
						Message = "Güncellenecek rol bulunamadı."
					};
				}

				// Mevcut entity üzerinde güncelleme yapın.
				mappingToUpdate.RoleName = request.RoleName;
				mappingToUpdate.ProcessTagID = request.ProcessTagID;
				mappingToUpdate.SourceApi = request.SourceApi;
				mappingToUpdate.IsActive = request.IsActive;
				// Eğer isterseniz, TargetApis alanını da endpoints'ten türetebilirsiniz:
				// mappingToUpdate.TargetApis = string.Join(",", request.Endpoints.Select(e => e.TargetApi));

				// Endpoints güncellemesi:
				// Mevcut endpoints koleksiyonunu temizleyin (silme işlemi de otomatik olarak tracking modunda gerçekleştirilecektir)
				mappingToUpdate.Endpoints.Clear();
				if (request.Endpoints != null && request.Endpoints.Count > 0)
				{
					foreach (var epDto in request.Endpoints)
					{
						mappingToUpdate.Endpoints.Add(new RoleMappingEndpoint
						{
							TargetApi = epDto.TargetApi,
							Endpoint = epDto.Endpoint,
							IsActive = epDto.IsActive
						});
					}
				}

				mappingToUpdate.WindchillAttributes.Clear();
				if (request.WindchillAttributes != null && request.WindchillAttributes.Any())
				{
					foreach (var attrName in request.WindchillAttributes)
					{
						mappingToUpdate.WindchillAttributes.Add(new RoleMappingAttribute
						{
							AttributeName = attrName.AttributeName,
							IsSelected = true
						});
					}
				}


				var updatedMapping = await _service.UpdateRoleMappingAsync(mappingToUpdate);
				return new UpdateRoleMappingResponse
				{
					Success = true,
					Message = "Rol güncelleme başarılı."
				};
			}
		}
	}
}
