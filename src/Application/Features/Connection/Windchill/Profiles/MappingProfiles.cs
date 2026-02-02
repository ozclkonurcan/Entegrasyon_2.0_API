using Application.Features.Connection.Sql.Commands.Update;
using Application.Features.Connection.Sql.Queries.GetList;
using Application.Features.Connection.Windchill.Commands.Update;
using Application.Features.Connection.Windchill.Queries.GetList;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Windchill.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<WindchillConnectionSettings, GetListWindchillConnectionListItemDto>().ReverseMap();
		CreateMap<WindchillConnectionSettings, UpdateWindchillConnectionCommand>().ReverseMap();
		CreateMap<WindchillConnectionSettings, UpdatedWindchillConnectionResponse>().ReverseMap();
	}
}
