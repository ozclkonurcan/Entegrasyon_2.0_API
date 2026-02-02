using Application.Features.WindchillIntegration.WTPartAlternateLink.Commands.Process;
using Application.Features.WindchillIntegration.WTPartAlternateLink.Queries.GetList;
using Application.Features.WindchillIntegration.WTPartAlternateLinkRemoved.Commands.Process;
using Application.Features.WindchillIntegration.WTPartAlternateLinkRemoved.Queries.GetList;
using Application.Features.WindchillIntegration.WTPartAlternateLinkRemoved.Queries.GetListError;
using Application.Features.WTParts.Queries.GetListAllAlternateLinkRemoved;
using AutoMapper;
using Domain.Entities.WTPartModels.AlternateModels;
using Domain.Entities.WTPartModels.AlternateRemovedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartAlternateLinkRemoved.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<WTPartAlternateLinkRemovedEntegration, ProcessWTPartAlternateLinkRemovedResponse>().ReverseMap();
		CreateMap<WTPartAlternateLinkRemovedEntegration, WTPartAlternateLinkRemovedSentEntegration>()
			.ForMember(dest => dest.LogID, opt => opt.Ignore())
			.ReverseMap();
		CreateMap<WTPartAlternateLinkRemovedEntegration, WTPartAlternateLinkRemovedErrorEntegration>()
			.ForMember(dest => dest.LogID, opt => opt.Ignore())
			.ReverseMap();

		CreateMap<WTPartAlternateLinkRemovedEntegration, GetListAllAlternateLinkRemovedWTPartListItemDto>().ReverseMap();


		//Sent Datas Mapping
		CreateMap<WTPartAlternateLinkRemovedSentEntegration, GetWTPartAlternateRemovedSentDatasDto>()
.ForMember(dest => dest.LogID, opt => opt.Ignore())
.ReverseMap();
		CreateMap<WTPartAlternateLinkRemovedSentEntegration, GetWTPartAlternateRemovedSentDatasQuery>().ReverseMap();

		CreateMap<ProcessWTPartAlternateLinkRemovedCommand, WTPartAlternateLinkRemovedErrorEntegration>().ReverseMap();
		CreateMap<WTPartAlternateLinkRemovedErrorEntegration, GetWTPartAlternateRemovedErrorDatasQuery>().ReverseMap();
		CreateMap<WTPartAlternateLinkRemovedErrorEntegration, GetWTPartAlternateRemovedErrorDatasDto>().ReverseMap();
	}
}
