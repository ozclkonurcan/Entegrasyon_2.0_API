using Application.Features.WTParts.Queries.GetListAll;
using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Interfaces.Generic;
using Application.Pipelines.Logging;
using Application.Requests;
using AutoMapper;
using Domain.Entities.WTPartModels.AlternateModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WTParts.Queries.GetListAllAlternateLink;

public class GetListAllAlternateLinkWTPartQuery : IRequest<List<GetListAllAlternateLinkWTPartListItemDto>>, ILoggableRequest
{
	public PageRequest PageRequest { get; set; }
	public string LogMessage => $"WTPart Muadil listeleme işlemi gerçekleştirildi.";


	public class GetListAllAlternateLinkWTPartQueryHandler : IRequestHandler<GetListAllAlternateLinkWTPartQuery, List<GetListAllAlternateLinkWTPartListItemDto>>
	{

		private readonly IGenericRepository<WTPartAlternateLinkEntegration> _genericWtpartAlternateRepository;
		private readonly IMapper _mapper;

		public GetListAllAlternateLinkWTPartQueryHandler(IGenericRepository<WTPartAlternateLinkEntegration> genericWtpartAlternateRepository, IMapper mapper)
		{
			_genericWtpartAlternateRepository = genericWtpartAlternateRepository;
			_mapper = mapper;
		}

		public async Task<List<GetListAllAlternateLinkWTPartListItemDto>> Handle(GetListAllAlternateLinkWTPartQuery request, CancellationToken cancellationToken)
		{
			//var wtParts = await _wTPartService.GetState();
			var wtPartAlternates = await _genericWtpartAlternateRepository.GetListAsync();


			var wtpartListDtoResponse = _mapper.Map<List<GetListAllAlternateLinkWTPartListItemDto>>(wtPartAlternates);

			return wtpartListDtoResponse;
		}
	}

}
