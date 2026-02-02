using Application.Interfaces.Generic;
using AutoMapper;
using Domain.Entities.WTPartModels.AlternateRemovedModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartAlternateLinkRemoved.Queries.GetListError;

public class GetWTPartAlternateRemovedErrorDatasQuery : IRequest<List<GetWTPartAlternateRemovedErrorDatasDto>>
{
	public class GetWTPartAlternateRemovedErrorDatasQueryHandler : IRequestHandler<GetWTPartAlternateRemovedErrorDatasQuery, List<GetWTPartAlternateRemovedErrorDatasDto>>
	{
		private readonly IGenericRepository<WTPartAlternateLinkRemovedErrorEntegration> _service;
		private readonly IMapper _mapper;
		public GetWTPartAlternateRemovedErrorDatasQueryHandler(IGenericRepository<WTPartAlternateLinkRemovedErrorEntegration> service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}
		public async Task<List<GetWTPartAlternateRemovedErrorDatasDto>> Handle(GetWTPartAlternateRemovedErrorDatasQuery request, CancellationToken cancellationToken)
		{
			var entities = await _service.GetListAsync();
			return _mapper.Map<List<GetWTPartAlternateRemovedErrorDatasDto>>(entities);
		}
	}


}
