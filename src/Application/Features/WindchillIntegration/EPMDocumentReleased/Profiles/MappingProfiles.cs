using Application.Features.WindchillIntegration.EPMDocumentReleased.Commands.Process;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocumentReleased.Profiles
{
	public class MappingProfiles : Profile
	{
		public MappingProfiles()
		{
			CreateMap<EPMDocument_RELEASED, ProcessEPMDocumentReleasedResponse>().ReverseMap();
		}
	}
}
