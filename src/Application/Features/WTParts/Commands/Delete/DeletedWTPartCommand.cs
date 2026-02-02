using Application.Interfaces.EntegrasyonModulu.WTPartServices;
using Application.Pipelines.Logging;
using AutoMapper;
using Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WTParts.Commands.Delete;

public class DeletedWTPartCommand:IRequest<DeletedWTPartResponse>,ILoggableRequest
{
	public long ParcaPartID { get; set; }
	public string LogMessage => $"WTPart silme işlemi gerçekleştirildi.";


	public class DeletedWTPartCommandHandler : IRequestHandler<DeletedWTPartCommand, DeletedWTPartResponse>
	{
		private readonly IWTPartService<WTPart> _wTPartService;
		private readonly IMapper _mapper;

		public DeletedWTPartCommandHandler(IMapper mapper, IWTPartService<WTPart> wTPartService)
		{
			_mapper = mapper;
			_wTPartService = wTPartService;
		}

		public async Task<DeletedWTPartResponse> Handle(DeletedWTPartCommand request, CancellationToken cancellationToken)
		{
			WTPart? wTPart = await _wTPartService.GetAsync(null,predicate: b => b.ParcaPartID == request.ParcaPartID, cancellationToken: cancellationToken);

			await _wTPartService.DeleteAsync(null,wTPart);

			DeletedWTPartResponse response = _mapper.Map<DeletedWTPartResponse>(wTPart);
			return response;
		}
	}
}
