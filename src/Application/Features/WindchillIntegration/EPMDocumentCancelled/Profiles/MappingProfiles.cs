using Application.Features.WindchillIntegration.EPMDocumentCancelled.Commands.Process;
using Application.Features.WindchillIntegration.EPMDocuments.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.EPMModels;
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

		CreateMap<EPMDocument_CANCELLED, GetEPMDocumentListItemDto>();
		CreateMap<EPMDocument_CANCELLED_SENT, GetEPMDocumentListItemDto>();
		CreateMap<EPMDocument_CANCELLED_ERROR, GetEPMDocumentListItemDto>();
	}
}
