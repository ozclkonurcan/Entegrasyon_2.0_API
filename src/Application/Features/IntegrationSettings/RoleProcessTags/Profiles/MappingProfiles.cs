using Application.Features.IntegrationSettings.ModuleSettings.Queries.GetById;
using Application.Features.IntegrationSettings.ModuleSettings.Queries.GetList;
using Application.Features.IntegrationSettings.RoleMappings.Commands.Create;
using Application.Features.IntegrationSettings.RoleMappings.Commands.Delete;
using Application.Features.IntegrationSettings.RoleMappings.Commands.Update;
using Application.Features.IntegrationSettings.RoleMappings.Queries.GetList;
using Application.Features.IntegrationSettings.RoleProcessTags.Queries.GetById;
using Application.Features.IntegrationSettings.RoleProcessTags.Queries.GetList;
using AutoMapper;
using Domain.Entities.IntegrationSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.RoleProcessTags.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		// Rol Mapping ve Endpoints eşlemeleri (önceki örneklerde olduğu gibi)
		CreateMap<RoleMapping, GetRoleMappingsDto>().ReverseMap();
		CreateMap<RoleMappingEndpoint, RoleMappingEndpointDto>().ReverseMap();

		// Yeni: RoleProcessTag eşlemesi
		CreateMap<RoleProcessTag, GetRoleProcessTagsDto>().ReverseMap();
		CreateMap<RoleProcessTag, GetRoleProcessTagByIdDto>().ReverseMap();

	}
}
