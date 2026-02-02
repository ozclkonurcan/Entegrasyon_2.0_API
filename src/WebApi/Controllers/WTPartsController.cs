using Application.Features.WindchillIntegration.WTPartAlternateLink.Queries.GetList;
using Application.Features.WindchillIntegration.WTPartAlternateLink.Queries.GetListAllLog;
using Application.Features.WindchillIntegration.WTPartAlternateLink.Queries.GetListError;
using Application.Features.WindchillIntegration.WTPartAlternateLinkRemoved.Queries.GetList;
using Application.Features.WindchillIntegration.WTPartAlternateLinkRemoved.Queries.GetListError;
using Application.Features.WindchillIntegration.WTPartLog.Queries.GetFilteredList;
using Application.Features.WindchillIntegration.WTPartLog.Queries.GetList;
using Application.Features.WindchillIntegration.WTPartLog.Queries.GetListAllLog;
using Application.Features.WindchillIntegration.WTPartLog.Queries.GetListError;
using Application.Features.WTParts.Queries.GetList;
using Application.Features.WTParts.Queries.GetListAll;
using Application.Features.WTParts.Queries.GetListAllAlternateLink;
using Application.Features.WTParts.Queries.GetListAllAlternateLinkRemoved;
using Application.Requests;
using Application.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WTPartsController : BaseController
{

	[HttpGet("sentdatas")]
	public async Task<ActionResult<List<GetWTPartSentDatasDto>>> GetWTPartSentDatas()
	{
		var result = await Mediator.Send(new GetWTPartSentDatasQuery());
		return Ok(result);
	}

	[HttpGet("errordatas")]
	public async Task<ActionResult<List<GetWTPartErrorDatasDto>>> GetWTPartErrorDatas()
	{
		var result = await Mediator.Send(new GetWTPartErrorDatasQuery());
		return Ok(result);
	}

	[HttpGet("sentdatasalternate")]
	public async Task<ActionResult<List<GetWTPartAlternateSentDatasDto>>> GetWTPartAlternateSentDatas()
	{
		var result = await Mediator.Send(new GetWTPartAlternateSentDatasQuery());
		return Ok(result);
	}

	[HttpGet("errordatasalternate")]
	public async Task<ActionResult<List<GetWTPartAlternateErrorDatasDto>>> GetWTPartAlternateErrorDatas()
	{
		var result = await Mediator.Send(new GetWTPartAlternateErrorDatasQuery());
		return Ok(result);
	}

	[HttpGet("sentdatasalternateremoved")]
	public async Task<ActionResult<List<GetWTPartAlternateRemovedSentDatasDto>>> GetWTPartAlternateRemovedSentDatas()
	{
		var result = await Mediator.Send(new GetWTPartAlternateRemovedSentDatasQuery());
		return Ok(result);
	}
	[HttpGet("errordatasalternateremoved")]
	public async Task<ActionResult<List<GetWTPartAlternateRemovedErrorDatasDto>>> GetWTPartAlternateRemovedErrorDatas()
	{
		var result = await Mediator.Send(new GetWTPartAlternateRemovedErrorDatasQuery());
		return Ok(result);
	}


	[HttpGet("filtered")]
	public async Task<IActionResult> GetFilteredSentDatas(
		   [FromQuery] string filterType = "daily",
		   [FromQuery] DateTime? startDate = null,
		   [FromQuery] DateTime? endDate = null,
		   [FromQuery] string searchText = "")
	{
		var query = new GetWTPartSentDatasFilteredQuery
		{
			FilterType = filterType,
			StartDate = startDate,
			EndDate = endDate,
			SearchText = searchText
		};

		var result = await Mediator.Send(query);
		return Ok(result);
	}
	[HttpGet("getlistall")]
	public async Task<IActionResult> GetListAll()
	{
		GetListAllWTPartQuery getListAllWTPartQuery = new();
		List<GetListAllWTPartListItemDto> response = await Mediator.Send(getListAllWTPartQuery);
		return Ok(response);
	}

	[HttpGet("getlistwithpaginate")]
	public async Task<IActionResult> GetListAsync([FromQuery] PageRequest pageResult)
	{
		GetListWTPartQuery getListWTPartQuery = new() { PageRequest = pageResult };
		GetListResponse<GetListWTPartListItemDto> response = await Mediator.Send(getListWTPartQuery);
		return Ok(response);
	}


	//[HttpGet("getlistalllogs")]
	//public async Task<IActionResult> GetListAllLogs([FromQuery] PageRequest pageRequest)
	//{
	//	GetWTPartAllLogsQuery query = new() { PageRequest = pageRequest };
	//	GetListResponse<GetWTPartAllLogsDto> response = await Mediator.Send(query);
	//	return Ok(response);
	//}

	[HttpGet("getlistalllogs")]
	public async Task<IActionResult> GetListAllLogs(
	[FromQuery] PageRequest pageRequest,
	[FromQuery] string? searchQuery,
	[FromQuery] DateTime? startDate,
	[FromQuery] DateTime? endDate)
	{
		GetWTPartAllLogsQuery query = new()
		{
			PageRequest = pageRequest,
			SearchQuery = searchQuery,
			StartDate = startDate,
			EndDate = endDate
		};

		GetListResponse<GetWTPartAllLogsDto> response = await Mediator.Send(query);
		return Ok(response);
	}



	[HttpGet("getlistallalternatelogs")]
	public async Task<IActionResult> GetListAllAlternateLogs(
	[FromQuery] PageRequest pageRequest,
	[FromQuery] string? searchQuery,
	[FromQuery] DateTime? startDate,
	[FromQuery] DateTime? endDate)
	{
		GetWTPartAlternateAllLogsQuery query = new()
		{
			PageRequest = pageRequest,
			SearchQuery = searchQuery,
			StartDate = startDate,
			EndDate = endDate
		};

		GetListResponse<GetWTPartAlternateAllLogsDto> response = await Mediator.Send(query);
		return Ok(response);
	}


	[HttpGet("getlistallalternatelink")]
	public async Task<IActionResult> GetListAllAlternateLink()
	{
		GetListAllAlternateLinkWTPartQuery getListAllAlternateLinkWTPartQuery = new();
		List<GetListAllAlternateLinkWTPartListItemDto> response = await Mediator.Send(getListAllAlternateLinkWTPartQuery);
		return Ok(response);
	}


	[HttpGet("getlistallalternatelinkRemoved")]
	public async Task<IActionResult> GetListAllAlternateLinkRemoved()
	{
		GetListAllAlternateLinkRemovedWTPartQuery getListAllAlternateLinkRemovedWTPartQuery = new();
		List<GetListAllAlternateLinkRemovedWTPartListItemDto> response = await Mediator.Send(getListAllAlternateLinkRemovedWTPartQuery);
		return Ok(response);
	}

}
