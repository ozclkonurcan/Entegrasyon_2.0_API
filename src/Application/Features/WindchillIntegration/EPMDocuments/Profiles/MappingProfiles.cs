using Application.Features.WindchillIntegration.EPMDocuments.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Entities.EPMModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocuments.Profiles
{
	public class MappingProfiles : Profile
	{
		public MappingProfiles()
		{
			CreateMap<EPMDocument_RELEASED, GetEPMDocumentListItemDto>();
			CreateMap<EPMDocument_SENT, GetEPMDocumentListItemDto>();
			CreateMap<EPMDocument_ERROR, GetEPMDocumentListItemDto>();
		}
	}
}
