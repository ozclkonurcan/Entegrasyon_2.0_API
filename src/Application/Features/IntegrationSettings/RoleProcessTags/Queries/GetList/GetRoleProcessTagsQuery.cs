using Application.Interfaces.IntegrationSettings;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleProcessTags.Queries.GetList;

public class GetRoleProcessTagsQuery :  IRequest<List<GetRoleProcessTagsDto>>
{
	public class GetRoleProcessTagsQueryHandler : IRequestHandler<GetRoleProcessTagsQuery, List<GetRoleProcessTagsDto>>
	{
		private readonly IIntegrationSettingsService _service;
		private readonly IMapper _mapper;

		public GetRoleProcessTagsQueryHandler(IIntegrationSettingsService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		public async Task<List<GetRoleProcessTagsDto>> Handle(GetRoleProcessTagsQuery request, CancellationToken cancellationToken)
		{
			var tags = await _service.GetRoleProcessTagsAsync();
			return _mapper.Map<List<GetRoleProcessTagsDto>>(tags);
		}
	}
}
