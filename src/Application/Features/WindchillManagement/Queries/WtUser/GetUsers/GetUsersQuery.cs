using Application.Interfaces.WindchillModule;
using Application.Pipelines.Logging;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillManagement.Queries.WtUser.GetUsers;

public class GetUsersQuery : IRequest<List<GetUsersItemDto>>, ILoggableRequest
{
	public string SearchTerm { get; set; }

	public string? LogMessage { get; set; }

	public class GetUsersQueryHandller : IRequestHandler<GetUsersQuery, List<GetUsersItemDto>>
	{
		private readonly IWindchillService _windchillService;
		private readonly IMapper _mapper;

		public GetUsersQueryHandller(IWindchillService windchillService, IMapper mapper)
		{
			_windchillService = windchillService;
			_mapper = mapper;
		}

		public async Task<List<GetUsersItemDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
		{

			var getUsers = await _windchillService.GetFindUserAsync(request.SearchTerm);

			var getUserItemDto = getUsers.Select(u => new GetUsersItemDto
			{
				Name = u.Name,
				EMail = u.EMail,
				FullName = u.FullName
			}).ToList();

			request.LogMessage = "Kullanici listelendi";

			return getUserItemDto;
		}
	}
}
