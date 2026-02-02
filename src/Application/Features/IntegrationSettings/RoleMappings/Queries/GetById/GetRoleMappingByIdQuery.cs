using Application.Interfaces.IntegrationSettings;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleMappings.Queries.GetById;

public class GetRoleMappingByIdQuery : IRequest<GetRoleMappingByIdDto>
{
	public int Id { get; set; }

	public class GetRoleMappingByIdQueryHandler : IRequestHandler<GetRoleMappingByIdQuery, GetRoleMappingByIdDto>
	{
		private readonly IIntegrationSettingsService _service;
		private readonly IMapper _mapper;

		public GetRoleMappingByIdQueryHandler(IIntegrationSettingsService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		public async Task<GetRoleMappingByIdDto> Handle(GetRoleMappingByIdQuery request, CancellationToken cancellationToken)
		{
			var roleMapping = await _service.GetRoleMappingByIdAsync(request.Id);
			return _mapper.Map<GetRoleMappingByIdDto>(roleMapping);
		}
	}
}
