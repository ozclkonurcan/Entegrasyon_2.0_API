using Application.Interfaces.Generic;
using AutoMapper;
using Domain.Entities.EPMModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocuments.Queries.GetListError
{
	public class GetErrorEPMDocumentReleasedQuery : IRequest<List<GetEPMDocumentListItemDto>>
	{
		public class Handler : IRequestHandler<GetErrorEPMDocumentReleasedQuery, List<GetEPMDocumentListItemDto>>
		{
			// Hatalılar tablosu: Des2_EPMDocument_Error
			private readonly IGenericRepository<EPMDocument_ERROR> _repository;
			private readonly IMapper _mapper;

			public Handler(IGenericRepository<EPMDocument_ERROR> repository, IMapper mapper)
			{
				_repository = repository;
				_mapper = mapper;
			}

			public async Task<List<GetEPMDocumentListItemDto>> Handle(GetErrorEPMDocumentReleasedQuery request, CancellationToken cancellationToken)
			{
				var data = await _repository.GetListAsync();
				return _mapper.Map<List<GetEPMDocumentListItemDto>>(data);
			}
		}
	}
}
