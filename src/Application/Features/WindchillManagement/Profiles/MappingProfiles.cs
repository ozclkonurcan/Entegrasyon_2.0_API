using Application.Features.WindchillManagement.Queries.WtToken.GetList;
using Application.Features.WindchillManagement.Queries.WtUser.GetUsers;
using AutoMapper;
using Domain.Entities.WindchillEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillManagement.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		//CreateMap<WrsToken, GetTokenItemDto>().ReverseMap();
		//CreateMap<WTUsers, GetUsersItemDto>().ReverseMap();
		//CreateMap<WTUsers, List<GetUsersItemDto>>().ReverseMap();

		CreateMap<WrsToken, GetTokenItemDto>().ReverseMap();
		CreateMap<WTUsers, GetUsersItemDto>().ReverseMap();
	}
}
