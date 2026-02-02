using Application.Features.WindchillIntegration.EPMDocumentCancelled.Commands.Process;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocumentCancelled.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<EPMDocument_CANCELLED, ProcessEPMDocumentCancelledResponse>().ReverseMap();
	}
}
