using Application.Features.Connection.Sql.Queries.GetList;
using Application.Features.Connection.Windchill.Rules;
using Application.Interfaces.ConnectionModule.WindchillConnectionModule;
using Application.Pipelines.Logging;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Windchill.Queries.GetList;

public class GetListWindchillConnectionQuery : IRequest<GetListWindchillConnectionListItemDto>,ILoggableRequest
{
	public string LogMessage { get; set; }

	public class GetListWindchillConnectionQueryHandler : IRequestHandler<GetListWindchillConnectionQuery, GetListWindchillConnectionListItemDto>
	{
		private readonly IWindchillConnectionService _connectionService;
		private readonly IMapper _mapper;

		public GetListWindchillConnectionQueryHandler(IWindchillConnectionService connectionService, IMapper mapper)
		{
			_connectionService = connectionService;
			_mapper = mapper;
		}

		public async Task<GetListWindchillConnectionListItemDto> Handle(GetListWindchillConnectionQuery request, CancellationToken cancellationToken)
		{
			var connections = await _connectionService.GetConnectionInformation();
			var connectionResponse = _mapper.Map<GetListWindchillConnectionListItemDto>(connections);
			request.LogMessage = "Kullanicilar listelendi";
			return connectionResponse;
		}
	}
}
