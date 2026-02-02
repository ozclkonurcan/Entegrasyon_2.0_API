using Application.Features.WindchillIntegration.WTPartAlternateLink.Commands.ErrorProcess;
using Application.Features.WindchillIntegration.WTPartAlternateLink.Commands.Process;
using Application.Features.WindchillIntegration.WTPartAlternateLink.Queries.GetList;
using Application.Features.WindchillIntegration.WTPartAlternateLink.Queries.GetListAllLog;
using Application.Features.WindchillIntegration.WTPartAlternateLink.Queries.GetListError;
using Application.Features.WindchillIntegration.WTPartLog.Queries.GetListAllLog;
using Application.Paging;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.WTPartModels.AlternateModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartAlternateLink.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<WTPartAlternateLinkEntegration, ProcessWTPartAlternateLinkResponse>().ReverseMap();
		CreateMap<WTPartAlternateLinkEntegration, WTPartAlternateLinkSentEntegration>()
			.ForMember(dest => dest.LogID, opt => opt.Ignore())
			.ReverseMap();
		CreateMap<WTPartAlternateLinkEntegration, WTPartAlternateLinkErrorEntegration>()
			.ForMember(dest => dest.LogID, opt => opt.Ignore())
			.ReverseMap();


		CreateMap<WTPartAlternateLinkErrorEntegration, WTPartAlternateLinkSentEntegration>()
	.ForMember(dest => dest.LogID, opt => opt.Ignore())
	.ReverseMap();


		CreateMap<WTPartAlternateLinkErrorEntegration, ErrorProcessWTPartAlternateLinkResponse>().ReverseMap();




		//Sentd Datas Mapping
		CreateMap<WTPartAlternateLinkSentEntegration, GetWTPartAlternateSentDatasDto>()
	.ForMember(dest => dest.LogID, opt => opt.Ignore())
	.ReverseMap();
		CreateMap<WTPartAlternateLinkSentEntegration, GetWTPartAlternateSentDatasQuery>().ReverseMap();
		CreateMap<WTPartAlternateLinkErrorEntegration, GetWTPartAlternateErrorDatasQuery>().ReverseMap();
		CreateMap<WTPartAlternateLinkErrorEntegration, GetWTPartAlternateErrorDatasDto>().ReverseMap();

		CreateMap<ProcessWTPartAlternateLinkCommand, WTPartAlternateLinkErrorEntegration>().ReverseMap();


		CreateMap<WTPartAlternateLinkLogEntegration, GetWTPartAlternateAllLogsDto>().ReverseMap();
		CreateMap<Paginate<WTPartAlternateLinkLogEntegration>, GetListResponse<GetWTPartAlternateAllLogsDto>>().ReverseMap();
	}
}
