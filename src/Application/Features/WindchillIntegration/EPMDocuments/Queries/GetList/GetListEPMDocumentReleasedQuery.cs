using Application.Interfaces.Generic;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocuments.Queries.GetList;

public class GetListEPMDocumentReleasedQuery : IRequest<List<GetEPMDocumentListItemDto>>
{
	public class Handler : IRequestHandler<GetListEPMDocumentReleasedQuery, List<GetEPMDocumentListItemDto>>
	{
		// Bekleyenler tablosu: Des2_EPMDocument
		private readonly IGenericRepository<EPMDocument_RELEASED> _repository;
		private readonly IMapper _mapper;

		public Handler(IGenericRepository<EPMDocument_RELEASED> repository, IMapper mapper)
		{
			_repository = repository;
			_mapper = mapper;
		}

		public async Task<List<GetEPMDocumentListItemDto>> Handle(GetListEPMDocumentReleasedQuery request, CancellationToken cancellationToken)
		{
			var data = await _repository.GetListAsync();
			return _mapper.Map<List<GetEPMDocumentListItemDto>>(data);
		}
	}
}
