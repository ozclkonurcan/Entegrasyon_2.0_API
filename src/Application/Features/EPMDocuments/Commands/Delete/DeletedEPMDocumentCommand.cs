//using Application.Interfaces.Generic; // Generic Repo buradan geliyor
//using Application.Pipelines.Logging;
//using AutoMapper;
//using Domain.Entities;
//using MediatR;
//using System;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Application.Features.EPMDocuments.Commands.Delete;

//public class DeletedEPMDocumentCommand : IRequest<DeletedEPMDocumentResponse>, ILoggableRequest
//{
//	public long EPMDocID { get; set; }
//	public string LogMessage => $"EPMDocument silme işlemi gerçekleştirildi.";

//	public class DeletedEPMDocumentCommandHandler : IRequestHandler<DeletedEPMDocumentCommand, DeletedEPMDocumentResponse>
//	{
//		// Generic Repository kullanıyoruz
//		private readonly IGenericRepository<EPMDocument> _ePMDocumentRepository;
//		private readonly IMapper _mapper;

//		public DeletedEPMDocumentCommandHandler(IMapper mapper, IGenericRepository<EPMDocument> ePMDocumentRepository)
//		{
//			_mapper = mapper;
//			_ePMDocumentRepository = ePMDocumentRepository;
//		}

//		public async Task<DeletedEPMDocumentResponse> Handle(DeletedEPMDocumentCommand request, CancellationToken cancellationToken)
//		{
//			// DÜZELTME: Generic Repository'de ilk parametre 'predicate'tir. 
//			// WTPartService'deki gibi 'null' (context) göndermiyoruz.

//			EPMDocument? ePMDoc = await _ePMDocumentRepository.GetAsync(
//				predicate: b => b.EPMDocID == request.EPMDocID,
//				cancellationToken: cancellationToken
//			);

//			// Veri bulunamazsa ne olacağını buraya ekleyebilirsin (Opsiyonel ama önerilir)
//			// if (ePMDoc == null) throw new BusinessException("Belge bulunamadı.");

//			// DÜZELTME: DeleteAsync sadece entity ve permanent bilgisi ister. 'null' silindi.
//			await _ePMDocumentRepository.DeleteAsync(ePMDoc);

//			DeletedEPMDocumentResponse response = _mapper.Map<DeletedEPMDocumentResponse>(ePMDoc);
//			return response;
//		}
//	}
//}