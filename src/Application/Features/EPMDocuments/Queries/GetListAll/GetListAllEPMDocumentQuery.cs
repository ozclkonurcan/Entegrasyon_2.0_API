using Application.Interfaces.Generic;
using Application.Pipelines.Logging;
using Application.Requests;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.EPMDocuments.Queries.GetListAll
{
	public class GetListAllEPMDocumentQuery : IRequest<List<GetListAllEPMDocumentListItemDto>>,ILoggableRequest
	{
		public PageRequest PageRequest { get; set; }
		public string LogMessage => $"EPMDocument listeleme işlemi gerçekleştirildi.";

		public class GetListAllEPMDocumentQueryHandler : IRequestHandler<GetListAllEPMDocumentQuery, List<GetListAllEPMDocumentListItemDto>>
		{

			private readonly IGenericRepository<EPMDocument> _ePMDocumentRepository;
			private readonly IMapper _mapper;

			public GetListAllEPMDocumentQueryHandler(IGenericRepository<EPMDocument> ePMDocumentRepository, IMapper mapper)
			{
				_ePMDocumentRepository = ePMDocumentRepository;
				_mapper = mapper;
			}

			public async Task<List<GetListAllEPMDocumentListItemDto>> Handle(GetListAllEPMDocumentQuery request, CancellationToken cancellationToken)
			{
				//var wtParts = await _wTPartService.GetState();
				var ePMDocuments = await _ePMDocumentRepository.GetState();


				var response = _mapper.Map<List<GetListAllEPMDocumentListItemDto>>(ePMDocuments);

				return response;
			}
		}
	}
}
