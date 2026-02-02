using Application.Interfaces.IntegrationSettings;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.ModuleSettings.Queries.GetList;

public class GetModuleSettingsQuery : IRequest<List<GetModuleSettingsDto>>
{
	public class GetModuleSettingsQueryHandler : IRequestHandler<GetModuleSettingsQuery, List<GetModuleSettingsDto>>
	{
		private readonly IIntegrationSettingsService _service;
		private readonly IMapper _mapper;

		public GetModuleSettingsQueryHandler(IIntegrationSettingsService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}
		public async Task<List<GetModuleSettingsDto>> Handle(GetModuleSettingsQuery request, CancellationToken cancellationToken)
		{
			var moduleSettings = await _service.GetIntegrationModuleSettingsAsync();
			return _mapper.Map<List<GetModuleSettingsDto>>(moduleSettings);
		}
	}
}
