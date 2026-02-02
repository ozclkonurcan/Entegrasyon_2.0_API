using Application.Features.IntegrationSettings.ModuleSettings.Queries.GetById;
using Application.Features.IntegrationSettings.ModuleSettings.Queries.GetList;
using Application.Features.IntegrationSettings.RoleMappings.Commands.Create;
using Application.Features.IntegrationSettings.RoleMappings.Commands.Delete;
using Application.Features.IntegrationSettings.RoleMappings.Commands.Update;
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

namespace Application.Features.IntegrationSettings.ModuleSettings.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<IntegrationModuleSettings, CreateModuleSettingsResponse>().ReverseMap();
		CreateMap<IntegrationModuleSettings, DeleteModuleSettingsResponse>().ReverseMap();
		CreateMap<IntegrationModuleSettings, UpdateModuleSettingsResponse>().ReverseMap();
		CreateMap<IntegrationModuleSettings, GetModuleSettingsByIdDto>().ReverseMap();
		CreateMap<IntegrationModuleSettings, GetModuleSettingsDto>().ReverseMap();

	}
}
