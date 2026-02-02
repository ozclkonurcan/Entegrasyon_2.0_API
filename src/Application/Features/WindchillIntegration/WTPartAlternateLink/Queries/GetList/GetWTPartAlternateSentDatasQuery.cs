using Application.Interfaces.Generic;
using AutoMapper;
using Domain.Entities.WTPartModels.AlternateModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartAlternateLink.Queries.GetList;

public class GetWTPartAlternateSentDatasQuery : IRequest<List<GetWTPartAlternateSentDatasDto>>
{
	public class GetWTPartAlternateSentDatasQueryHandler : IRequestHandler<GetWTPartAlternateSentDatasQuery, List<GetWTPartAlternateSentDatasDto>>
	{
		private readonly IGenericRepository<WTPartAlternateLinkSentEntegration> _service;
		private readonly IMapper _mapper;
		public GetWTPartAlternateSentDatasQueryHandler(IGenericRepository<WTPartAlternateLinkSentEntegration> service, IMapper mapper)
		{
			_service = service;
			_mapper = mapper;
		}
		public async Task<List<GetWTPartAlternateSentDatasDto>> Handle(GetWTPartAlternateSentDatasQuery request, CancellationToken cancellationToken)
		{
			var entities = await _service.GetListAsync();
			return _mapper.Map<List<GetWTPartAlternateSentDatasDto>>(entities);
		}
		}
}

