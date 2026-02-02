using Application.Interfaces.ConnectionModule;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Sql.Queries.GetList;

public class GetListConnectionQuery : IRequest<GetListConnectionListItemDto>
{
	public class GetListConnectionQueryHandler : IRequestHandler<GetListConnectionQuery, GetListConnectionListItemDto>
	{
		private readonly IConnectionService _connectionService;
		private readonly IMapper _mapper;

		public GetListConnectionQueryHandler(IConnectionService connectionService, IMapper mapper)
		{
			_connectionService = connectionService;
			_mapper = mapper;
		}

		public async Task<GetListConnectionListItemDto> Handle(GetListConnectionQuery request, CancellationToken cancellationToken)
		{
			var connections = await _connectionService.GetConnectionInformation();
			var connectionResponse = _mapper.Map<GetListConnectionListItemDto>(connections);
			return connectionResponse;
		}
	}
}
