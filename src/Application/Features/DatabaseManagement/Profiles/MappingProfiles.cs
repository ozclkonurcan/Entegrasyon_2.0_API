using Application.Features.DatabaseManagement.Commands.Create;
using Application.Features.DatabaseManagement.Queries.GetList;
using Application.Features.DatabaseManagement.Queries.TableControls;
using Application.Paging;
using Application.Responses;
using AutoMapper;
using Domain.Entities.DatabaseManagement;
using System.Collections.Generic;

namespace Application.Features.DatabaseManagement.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		// DatabaseManagementDefinations -> CreatedDatabaseResponse
		CreateMap<DatabaseManagementDefinations, CreatedDatabaseResponse>().ReverseMap();
		CreateMap<DatabaseManagementDefinations, CreateDatabaseCommand>().ReverseMap();

		CreateMap<DatabaseManagementDefinations, TableControlsDatabaseListItemDto>().ReverseMap();
		//CreateMap<TableControlsDatabaseListItemDto, DatabaseManagementDefinations>().ReverseMap();


		// DatabaseManagementDefinations -> GetListDatabaseListItemDto
		CreateMap<DatabaseManagementDefinations, CreatedDatabaseResponse>()
			.ForMember(dest => dest.TableTitle, opt => opt.MapFrom(src => src.TableTitle))
			.ForMember(dest => dest.TableName, opt => opt.MapFrom(src => src.TableName))
			.ForMember(dest => dest.TableSchema, opt => opt.MapFrom(src => src.TableSchema))
			.ForMember(dest => dest.CreateQuery, opt => opt.MapFrom(src => src.CreateQuery))
			.ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
			.ForMember(dest => dest.Triggers, opt => opt.MapFrom(src => src.Triggers))
			.ReverseMap();

		// GetListResponse<DatabaseManagementDefinations> -> GetListResponse<GetListDatabaseListItemDto>
		CreateMap<GetListResponse<DatabaseManagementDefinations>, GetListResponse<CreatedDatabaseResponse>>()
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
			.ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count));

		// Paginate<DatabaseManagementDefinations> -> GetListResponse<GetListDatabaseListItemDto>
		CreateMap<Paginate<DatabaseManagementDefinations>, GetListResponse<CreatedDatabaseResponse>>()
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
			.ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count));

		// DatabaseManagementDefinations -> GetListDatabaseListItemDto
		CreateMap<DatabaseManagementDefinations, GetListDatabaseListItemDto>()
			.ForMember(dest => dest.TableTitle, opt => opt.MapFrom(src => src.TableTitle))
			.ForMember(dest => dest.TableName, opt => opt.MapFrom(src => src.TableName))
			.ForMember(dest => dest.TableSchema, opt => opt.MapFrom(src => src.TableSchema))
			.ForMember(dest => dest.CreateQuery, opt => opt.MapFrom(src => src.CreateQuery))
			.ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
			.ForMember(dest => dest.Triggers, opt => opt.MapFrom(src => src.Triggers))
			.ReverseMap();

		// GetListResponse<DatabaseManagementDefinations> -> GetListResponse<GetListDatabaseListItemDto>
		CreateMap<GetListResponse<DatabaseManagementDefinations>, GetListResponse<GetListDatabaseListItemDto>>()
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
			.ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count));

		// Paginate<DatabaseManagementDefinations> -> GetListResponse<GetListDatabaseListItemDto>
		CreateMap<Paginate<DatabaseManagementDefinations>, GetListResponse<GetListDatabaseListItemDto>>()
			.ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
			.ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count));
	}
}