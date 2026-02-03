using Application.Features.WindchillIntegration.EPMDocuments.Queries;
using Application.Interfaces.Generic;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocumentCancelled.Queries.GetList
{
	public class GetListEPMDocumentCancelledQuery : IRequest<List<GetEPMDocumentListItemDto>>
	{
		public class Handler : IRequestHandler<GetListEPMDocumentCancelledQuery, List<GetEPMDocumentListItemDto>>
		{
			// Bekleyen Cancelled: Des2_EPMDocument_Cancelled
			private readonly IGenericRepository<EPMDocument_CANCELLED> _repository;
			private readonly IMapper _mapper;

			public Handler(IGenericRepository<EPMDocument_CANCELLED> repository, IMapper mapper)
			{
				_repository = repository;
				_mapper = mapper;
			}

			public async Task<List<GetEPMDocumentListItemDto>> Handle(GetListEPMDocumentCancelledQuery request, CancellationToken cancellationToken)
			{
				var data = await _repository.GetListAsync();
				return _mapper.Map<List<GetEPMDocumentListItemDto>>(data);
			}
		}
	}
}
