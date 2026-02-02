using Application.Features.WindchillIntegration.WTPartReleased.Commands.ErrorProcess;
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

namespace Application.Features.WindchillIntegration.WTPartReleased.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<WTPart, ProcessWTPartReleasedResponse>().ReverseMap();
		CreateMap<WTPartError, ErrorProcessWTPartReleasedResponse>().ReverseMap();

	}
}
