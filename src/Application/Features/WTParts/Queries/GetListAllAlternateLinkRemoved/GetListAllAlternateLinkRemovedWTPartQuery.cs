using Application.Features.WTParts.Queries.GetListAllAlternateLink;
using Application.Interfaces.Generic;
using Application.Pipelines.Logging;
using Application.Requests;
using AutoMapper;
using Domain.Entities.WTPartModels.AlternateRemovedModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WTParts.Queries.GetListAllAlternateLinkRemoved;

public class GetListAllAlternateLinkRemovedWTPartQuery : IRequest<List<GetListAllAlternateLinkRemovedWTPartListItemDto>>, ILoggableRequest
{
	public PageRequest PageRequest { get; set; }
	public string LogMessage => $"WTPart Silinen Muadil listeleme işlemi gerçekleştirildi.";

	public class GetListAllAlternateLinkRemovedWTPartQueryHandler : IRequestHandler<GetListAllAlternateLinkRemovedWTPartQuery, List<GetListAllAlternateLinkRemovedWTPartListItemDto>>
	{
		private readonly IGenericRepository<WTPartAlternateLinkRemovedEntegration> _genericWtpartAlternateRemovedRepository;
		private readonly IMapper _mapper;

		public GetListAllAlternateLinkRemovedWTPartQueryHandler(IGenericRepository<WTPartAlternateLinkRemovedEntegration> genericWtpartAlternateRemovedRepository, IMapper mapper)
		{
			_genericWtpartAlternateRemovedRepository = genericWtpartAlternateRemovedRepository;
			_mapper = mapper;
		}

		public async Task<List<GetListAllAlternateLinkRemovedWTPartListItemDto>> Handle(GetListAllAlternateLinkRemovedWTPartQuery request, CancellationToken cancellationToken)
		{
			var wtPartAlternates = await _genericWtpartAlternateRemovedRepository.GetListAsync();


			var wtpartListDtoResponse = _mapper.Map<List<GetListAllAlternateLinkRemovedWTPartListItemDto>>(wtPartAlternates);

			return wtpartListDtoResponse;
		}
	}
}



