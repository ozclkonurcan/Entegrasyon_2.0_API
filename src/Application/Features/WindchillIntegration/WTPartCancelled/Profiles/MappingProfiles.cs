using Application.Features.WindchillIntegration.WTPartCancelled.Commands.ErrorProcess;
using Application.Features.WindchillIntegration.WTPartCancelled.Commands.Process;
using Application.Features.WindchillIntegration.WTPartReleased.Commands.Process;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartCancelled.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<WTPart, ProcessWTPartCancelledResponse>().ReverseMap();
		CreateMap<WTPartError, ErrorProcessWTPartCancelledResponse>().ReverseMap();

	}
}
