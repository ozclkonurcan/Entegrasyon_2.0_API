using Application.Interfaces.Generic;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.EPMModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocuments.Queries.GetSentList;

public class GetSentEPMDocumentReleasedQuery : IRequest<List<GetEPMDocumentListItemDto>>
{
	public class Handler : IRequestHandler<GetSentEPMDocumentReleasedQuery, List<GetEPMDocumentListItemDto>>
	{
		// Gönderilenler tablosu: Des2_EPMDocument_Sent
		private readonly IGenericRepository<EPMDocument_SENT> _repository;
		private readonly IMapper _mapper;

		public Handler(IGenericRepository<EPMDocument_SENT> repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
		}

		public async Task<List<GetEPMDocumentListItemDto>> Handle(GetSentEPMDocumentReleasedQuery request, CancellationToken cancellationToken)
		{
			var data = await _repository.GetListAsync();
			return _mapper.Map<List<GetEPMDocumentListItemDto>>(data);
		}
	}
}
