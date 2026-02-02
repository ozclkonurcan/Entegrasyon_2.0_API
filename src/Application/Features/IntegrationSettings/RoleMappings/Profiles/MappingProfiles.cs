using Application.Features.IntegrationSettings.ModuleSettings.Commands.Create;
using Application.Features.IntegrationSettings.ModuleSettings.Commands.Delete;
using Application.Features.IntegrationSettings.ModuleSettings.Commands.Update;
using Application.Features.IntegrationSettings.RoleMappings.Queries.GetById;
using Application.Features.IntegrationSettings.RoleMappings.Queries.GetList;
using Application.Features.WTParts.Queries.GetList;
using Application.Paging;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.IntegrationSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleMappings.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<RoleMapping, CreateRoleMappingResponse>().ReverseMap();
		CreateMap<RoleMapping, DeleteRoleMappingResponse>().ReverseMap();
		CreateMap<RoleMapping, UpdateRoleMappingResponse>().ReverseMap();
		CreateMap<RoleMapping, GetRoleMappingByIdDto>().ReverseMap();
		CreateMap<RoleMapping, GetRoleMappingsDto>().ReverseMap();
		CreateMap<RoleMappingEndpoint, GetRoleMappingsDto>().ReverseMap();

		CreateMap<RoleMappingEndpoint, Queries.GetList.RoleMappingEndpointDto>().ReverseMap();
		CreateMap<RoleMappingEndpoint, Queries.GetById.RoleMappingEndpointDto>().ReverseMap();
	}
}
