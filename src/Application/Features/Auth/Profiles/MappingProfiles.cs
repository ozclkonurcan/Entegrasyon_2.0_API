using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<Security.Entities.RefreshToken<Guid, Guid>, RefreshToken>().ReverseMap();
		//CreateMap<RefreshToken, RevokedTokenResponse>().ReverseMap();
	}
}
