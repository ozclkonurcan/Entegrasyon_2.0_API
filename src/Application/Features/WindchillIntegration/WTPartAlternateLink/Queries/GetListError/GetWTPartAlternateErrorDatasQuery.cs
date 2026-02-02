using Application.Interfaces.Generic;
using AutoMapper;
using Domain.Entities.WTPartModels.AlternateModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartAlternateLink.Queries.GetListError;

public class GetWTPartAlternateErrorDatasQuery : IRequest<List<GetWTPartAlternateErrorDatasDto>>
{
	public class GetWTPartAlternateErrorDatasQueryHandler : IRequestHandler<GetWTPartAlternateErrorDatasQuery, List<GetWTPartAlternateErrorDatasDto>>
	{
		private readonly IGenericRepository<WTPartAlternateLinkErrorEntegration> _service;
		private readonly IMapper _mapper;
		public GetWTPartAlternateErrorDatasQueryHandler(IGenericRepository<WTPartAlternateLinkErrorEntegration> service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}
		public async Task<List<GetWTPartAlternateErrorDatasDto>> Handle(GetWTPartAlternateErrorDatasQuery request, CancellationToken cancellationToken)
		{
			var entities = await _service.GetListAsync();
			return _mapper.Map<List<GetWTPartAlternateErrorDatasDto>>(entities);
		}
	}
}
