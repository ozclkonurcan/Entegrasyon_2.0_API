using Application.Features.WindchillIntegration.EPMDocuments.Queries;
using Application.Interfaces.Generic;
using AutoMapper;
using Domain.Entities.EPMModels;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocumentCancelled.Queries.GetSentList
{
	public class GetSentEPMDocumentCancelledQuery : IRequest<List<GetEPMDocumentListItemDto>>
	{
		public class Handler : IRequestHandler<GetSentEPMDocumentCancelledQuery, List<GetEPMDocumentListItemDto>>
		{
			// Sent Cancelled: Des2_EPMDocument_Cancelled_Sent
			private readonly IGenericRepository<EPMDocument_CANCELLED_SENT> _repository;
			private readonly IMapper _mapper;

			public Handler(IGenericRepository<EPMDocument_CANCELLED_SENT> repository, IMapper mapper)
			{
				_repository = repository;
				_mapper = mapper;
			}

			public async Task<List<GetEPMDocumentListItemDto>> Handle(GetSentEPMDocumentCancelledQuery request, CancellationToken cancellationToken)
			{
				var data = await _repository.GetListAsync();
				return _mapper.Map<List<GetEPMDocumentListItemDto>>(data);
			}
		}
	}
}
