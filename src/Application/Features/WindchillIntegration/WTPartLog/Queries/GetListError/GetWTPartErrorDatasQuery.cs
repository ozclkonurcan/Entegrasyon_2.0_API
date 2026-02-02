using Application.Features.WindchillIntegration.WTPartLog.Queries.GetList;
using Application.Interfaces.Generic;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartLog.Queries.GetListError;

public class GetWTPartErrorDatasQuery : IRequest<List<GetWTPartErrorDatasDto>>
{
	public class GetWTPartErrorDatasQueryHandler : IRequestHandler<GetWTPartErrorDatasQuery, List<GetWTPartErrorDatasDto>>
	{
		private readonly IGenericRepository<WTPartError> _service;
		private readonly IMapper _mapper;

		public GetWTPartErrorDatasQueryHandler(IGenericRepository<WTPartError> service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		public async Task<List<GetWTPartErrorDatasDto>> Handle(GetWTPartErrorDatasQuery request, CancellationToken cancellationToken)
		{
			var entities = await _service.GetListAsync();
			return _mapper.Map<List<GetWTPartErrorDatasDto>>(entities);
		}
	}
}
