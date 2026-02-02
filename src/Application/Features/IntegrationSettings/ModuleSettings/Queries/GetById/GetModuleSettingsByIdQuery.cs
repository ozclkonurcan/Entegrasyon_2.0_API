using Application.Interfaces.IntegrationSettings;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.ModuleSettings.Queries.GetById;

public class GetModuleSettingsByIdQuery : IRequest<GetModuleSettingsByIdDto>
{
	public int Id { get; set; }
	public class GetModuleSettingsByIdQueryHandler : IRequestHandler<GetModuleSettingsByIdQuery, GetModuleSettingsByIdDto>
	{
		private readonly IIntegrationSettingsService _service;
		private readonly IMapper _mapper;

		public GetModuleSettingsByIdQueryHandler(IIntegrationSettingsService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		public async Task<GetModuleSettingsByIdDto> Handle(GetModuleSettingsByIdQuery request, CancellationToken cancellationToken)
		{
			var moduleSetting = await _service.GetIntegrationModuleSettingByIdAsync(request.Id);
			return _mapper.Map<GetModuleSettingsByIdDto>(moduleSetting);
		}
	}
}
