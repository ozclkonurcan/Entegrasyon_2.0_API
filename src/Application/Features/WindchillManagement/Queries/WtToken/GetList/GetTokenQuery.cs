using Application.Interfaces.WindchillModule;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillManagement.Queries.WtToken.GetList;

public class GetTokenQuery : IRequest<GetTokenItemDto>
{
	public class GetTokenQueryHandler : IRequestHandler<GetTokenQuery, GetTokenItemDto>
	{
		private readonly IWindchillService _service;
		private readonly IMapper _mapper;

		public GetTokenQueryHandler(IWindchillService service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		public async Task<GetTokenItemDto> Handle(GetTokenQuery request, CancellationToken cancellationToken)
		{
			var getToken = await _service.GetTokenAsync();
			var getTokenItemDto = _mapper.Map<GetTokenItemDto>(getToken);

			return getTokenItemDto;
		}
	}
}
