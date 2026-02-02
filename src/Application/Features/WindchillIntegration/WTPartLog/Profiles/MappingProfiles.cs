using Application.Features.WindchillIntegration.WTPartLog.Queries.GetFilteredList;
using Application.Features.WindchillIntegration.WTPartLog.Queries.GetList;
using Application.Features.WindchillIntegration.WTPartLog.Queries.GetListAllLog;
using Application.Features.WindchillIntegration.WTPartLog.Queries.GetListError;
using Application.Features.WindchillIntegration.WTPartReleased.Commands.Process;
using Application.Features.WTParts.Queries.GetList;
using Application.Paging;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartLog.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<WTPartSentDatas, GetWTPartSentDatasDto>().ReverseMap();
		CreateMap<WTPartSentDatas, GetWTPartSentDatasFilteredQuery>().ReverseMap();
		CreateMap<WTPartAllLogs, GetWTPartAllLogsDto>().ReverseMap();
		CreateMap<Paginate<WTPartAllLogs>, GetListResponse<GetWTPartAllLogsDto>>().ReverseMap();
		CreateMap<WTPartError, GetWTPartErrorDatasDto>().ReverseMap();

	}
}
