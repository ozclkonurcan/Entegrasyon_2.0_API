using Application.Features.EPMDocuments.Commands.Delete;
using Application.Features.EPMDocuments.Queries.GetList;
using Application.Paging;
using Application.Responses;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.EPMDocuments.Profiles;

public class MappingProfiles : Profile
{
	public MappingProfiles()
	{
		// 1. LİSTELEME İŞLEMLERİ (Query)
		// Veritabanı nesnesini (EPMDocument), Liste DTO'suna çevirir.
		CreateMap<EPMDocument, GetListEPMDocumentListItemDto>().ReverseMap();

		// **EN ÖNEMLİ KISIM BURASI**
		// Generic Repository'den gelen 'Paginate' yapısını, senin Response modeline çevirir.
		CreateMap<Paginate<EPMDocument>, GetListResponse<GetListEPMDocumentListItemDto>>().ReverseMap();


		// 2. SİLME İŞLEMLERİ (Command)
		// Silinen entity'yi (EPMDocument), silme response'una (DeletedEPMDocumentResponse) çevirir.
		CreateMap<EPMDocument, DeletedEPMDocumentResponse>().ReverseMap();


		// EKSTRA: İleride Detay (GetById) veya Ekleme/Güncelleme yaparsan onları da buraya ekleyeceğiz.
		// Örnek: CreateMap<EPMDocument, CreatedEPMDocumentResponse>().ReverseMap();
	}
}