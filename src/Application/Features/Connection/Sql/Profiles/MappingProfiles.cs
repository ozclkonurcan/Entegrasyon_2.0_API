using Application.Features.Connection.Sql.Commands.Update;
using Application.Features.Connection.Sql.Queries.GetList;
using Application.Features.Connection.Sql.Queries.SqlContorls;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Connection.Sql.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		CreateMap<ConnectionSettings, GetListConnectionListItemDto>().ReverseMap();
		CreateMap<ConnectionSettings, ConnectionControlListItemDto>().ReverseMap();
		CreateMap<ConnectionSettings, UpdateConnectionCommand>().ReverseMap();
		CreateMap<ConnectionSettings, UpdatedConnectionResponse>().ReverseMap();

		//Farklı class larda yani class ile DTO prop ları arasında isim farklılığı var ise gerekli eşleşştirmeyi böyle yapabilriz.
		//CreateMap<Model, GetListModelListItemDto>()
		//	.ForMember(destinationMember: c => c.BrandName, memberOptions: opt => opt.MapFrom(c => c.Brand.Name))
		//	.ForMember(destinationMember: c => c.FuelName, memberOptions: opt => opt.MapFrom(c => c.Fuel.Name))
		//	.ForMember(destinationMember: c => c.TransmissionName, memberOptions: opt => opt.MapFrom(c => c.Transmission.Name))
		//	.ReverseMap();

	}
}
