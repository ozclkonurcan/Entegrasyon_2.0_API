using Application.Interfaces.IntegrationSettings;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleProcessTags.Queries.GetById;

public class GetRoleProcessTagByIdQuery : IRequest<GetRoleProcessTagByIdDto>
{
	public int ProcessTagID { get; set; }

	public class GetRoleProcessTagByIdQueryHandler : IRequestHandler<GetRoleProcessTagByIdQuery, GetRoleProcessTagByIdDto>
	{
		private readonly IIntegrationSettingsService _service;
		private readonly IMapper _mapper;

		public GetRoleProcessTagByIdQueryHandler(IIntegrationSettingsService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		public async Task<GetRoleProcessTagByIdDto> Handle(GetRoleProcessTagByIdQuery request, CancellationToken cancellationToken)
		{
			var tag = await _service.GetRoleProcessTagByIdAsync(request.ProcessTagID);
			return _mapper.Map<GetRoleProcessTagByIdDto>(tag);
		}
	}
}