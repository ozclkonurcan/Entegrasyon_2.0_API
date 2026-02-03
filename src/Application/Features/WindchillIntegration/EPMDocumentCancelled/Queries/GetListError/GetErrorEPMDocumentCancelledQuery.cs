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

namespace Application.Features.WindchillIntegration.EPMDocumentCancelled.Queries.GetListError
{
	public class GetErrorEPMDocumentCancelledQuery : IRequest<List<GetEPMDocumentListItemDto>>
	{
		public class Handler : IRequestHandler<GetErrorEPMDocumentCancelledQuery, List<GetEPMDocumentListItemDto>>
		{
			// Error Cancelled: Des2_EPMDocument_Cancelled_Error
			private readonly IGenericRepository<EPMDocument_CANCELLED_ERROR> _repository;
			private readonly IMapper _mapper;

			public Handler(IGenericRepository<EPMDocument_CANCELLED_ERROR> repository, IMapper mapper)
			{
				_repository = repository;
				_mapper = mapper;
			}

			public async Task<List<GetEPMDocumentListItemDto>> Handle(GetErrorEPMDocumentCancelledQuery request, CancellationToken cancellationToken)
			{
				var data = await _repository.GetListAsync();
				return _mapper.Map<List<GetEPMDocumentListItemDto>>(data);
			}
		}
	}
}
