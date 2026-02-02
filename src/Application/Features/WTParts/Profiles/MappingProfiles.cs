using Application.Features.WTParts.Queries.GetList;
using Application.Features.WTParts.Queries.GetListAll;
using Application.Features.WTParts.Queries.GetListAllAlternateLink;
using Application.Features.WTParts.Queries.GetListAllAlternateLinkRemoved;
using Application.Paging;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.WTPartModels.AlternateModels;
using Domain.Entities.WTPartModels.AlternateRemovedModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WTParts.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<WTPart,GetListWTPartListItemDto>().ReverseMap();
		CreateMap<WTPart, GetListAllWTPartListItemDto>().ReverseMap();
		CreateMap<WTPart, List<GetListAllWTPartListItemDto>>().ReverseMap();
		CreateMap<Paginate<WTPart>, GetListResponse<GetListWTPartListItemDto>>().ReverseMap();
		//Alternate / Muadil
		CreateMap<WTPartAlternateLinkEntegration, GetListAllAlternateLinkWTPartListItemDto>().ReverseMap();
		CreateMap<WTPartAlternateLinkEntegration, List<GetListAllAlternateLinkWTPartListItemDto>>().ReverseMap();
		CreateMap<Paginate<WTPartAlternateLinkEntegration>, GetListResponse<GetListAllAlternateLinkWTPartListItemDto>>().ReverseMap();

		//Alternate Removed / Muadil Removed
		CreateMap<WTPartAlternateLinkRemoved, GetListAllAlternateLinkRemovedWTPartListItemDto>().ReverseMap();
		CreateMap<WTPartAlternateLinkRemoved, List<GetListAllAlternateLinkRemovedWTPartListItemDto>>().ReverseMap();
		CreateMap<Paginate<WTPartAlternateLinkRemoved>, GetListResponse<GetListAllAlternateLinkRemovedWTPartListItemDto>>().ReverseMap();



		//Farklı class larda yani class ile DTO prop ları arasında isim farklılığı var ise gerekli eşleşştirmeyi böyle yapabilriz.
		//CreateMap<Model, GetListModelListItemDto>()
		//	.ForMember(destinationMember: c => c.BrandName, memberOptions: opt => opt.MapFrom(c => c.Brand.Name))
		//	.ForMember(destinationMember: c => c.FuelName, memberOptions: opt => opt.MapFrom(c => c.Fuel.Name))
		//	.ForMember(destinationMember: c => c.TransmissionName, memberOptions: opt => opt.MapFrom(c => c.Transmission.Name))
		//	.ReverseMap();
	}
}
