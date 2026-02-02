using Application.Interfaces.IntegrationSettings;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleMappings.Queries.GetList;

public class GetRoleMappingsQuery : IRequest<List<GetRoleMappingsDto>>
{

	public class GetRoleMappingsQueryHandler : IRequestHandler<GetRoleMappingsQuery, List<GetRoleMappingsDto>>
	{
		private readonly IIntegrationSettingsService _service;
		private readonly IMapper _mapper;

		public GetRoleMappingsQueryHandler(IIntegrationSettingsService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		public async Task<List<GetRoleMappingsDto>> Handle(GetRoleMappingsQuery request, CancellationToken cancellationToken)
		{
			var roleMappings = await _service.GetRoleMappingsAsync();
			return _mapper.Map<List<GetRoleMappingsDto>>(roleMappings);
		}
	}
}
