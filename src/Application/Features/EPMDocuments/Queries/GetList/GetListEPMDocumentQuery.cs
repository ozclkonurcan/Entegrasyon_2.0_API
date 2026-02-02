using Application.Interfaces.Generic;
using Application.Pipelines.Logging;
using Application.Requests;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.EPMDocuments.Queries.GetList
{
	public class GetListEPMDocumentQuery : IRequest<GetListResponse<GetListEPMDocumentListItemDto>>, ILoggableRequest
	{
		public PageRequest PageRequest { get; set; }
		public string LogMessage => $"EPMDocument listeleme işlemi gerçekleştirildi.";

		public class GetListEPMDocumentQueryHandler : IRequestHandler<GetListEPMDocumentQuery, GetListResponse<GetListEPMDocumentListItemDto>>
		{
			// Burada gerekli servis veya repository'yi ekleyin
			// Örneğin: private readonly IEPMDocumentService _ePMDocumentService;

			private readonly IGenericRepository<EPMDocument> _ePMDocumentRepository;
			private readonly IMapper _mapper;
			public GetListEPMDocumentQueryHandler(IGenericRepository<EPMDocument> ePMDocumentRepository, IMapper mapper)
			{
				_ePMDocumentRepository = ePMDocumentRepository;
				_mapper = mapper;
			}
			public async Task<GetListResponse<GetListEPMDocumentListItemDto>> Handle(GetListEPMDocumentQuery request, CancellationToken cancellationToken)
			{

				var ePMDocuments = await _ePMDocumentRepository.GetListPaginationAsync(
						index: request.PageRequest.PageIndex,
						size: request.PageRequest.PageSize,
						cancellationToken: cancellationToken
						);


				var response = _mapper.Map<GetListResponse<GetListEPMDocumentListItemDto>>(ePMDocuments);

				return response;

			
			}
		}
	}
}
