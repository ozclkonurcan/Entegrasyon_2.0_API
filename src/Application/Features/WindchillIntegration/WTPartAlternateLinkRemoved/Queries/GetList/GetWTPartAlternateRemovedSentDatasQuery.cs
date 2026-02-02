using Application.Features.WindchillIntegration.WTPartAlternateLink.Queries.GetList;
using Application.Interfaces.Generic;
using AutoMapper;
using Domain.Entities.WTPartModels.AlternateRemovedModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartAlternateLinkRemoved.Queries.GetList;

public class GetWTPartAlternateRemovedSentDatasQuery : IRequest<List<GetWTPartAlternateRemovedSentDatasDto>>
{
	public class GetWTPartAlternateRemovedSentDatasQueryHandler : IRequestHandler<GetWTPartAlternateRemovedSentDatasQuery, List<GetWTPartAlternateRemovedSentDatasDto>>
	{
		private readonly IGenericRepository<WTPartAlternateLinkRemovedSentEntegration> _service;
		private readonly IMapper _mapper;

		public GetWTPartAlternateRemovedSentDatasQueryHandler(IGenericRepository<WTPartAlternateLinkRemovedSentEntegration> service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}

		public async Task<List<GetWTPartAlternateRemovedSentDatasDto>> Handle(GetWTPartAlternateRemovedSentDatasQuery request, CancellationToken cancellationToken)
		{
			var entities = await _service.GetListAsync();
			return _mapper.Map<List<GetWTPartAlternateRemovedSentDatasDto>>(entities);
		}
	}
}
