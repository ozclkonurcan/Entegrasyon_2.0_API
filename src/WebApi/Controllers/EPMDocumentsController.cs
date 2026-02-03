
// Equivalence namespace'lerini de ekle...
using Application.Features.WindchillIntegration.EPMDocumentCancelled.Queries.GetList;
using Application.Features.WindchillIntegration.EPMDocumentCancelled.Queries.GetListError;
using Application.Features.WindchillIntegration.EPMDocumentCancelled.Queries.GetSentList;
using Application.Features.WindchillIntegration.EPMDocuments.Queries;
using Application.Features.WindchillIntegration.EPMDocuments.Queries.GetList;
using Application.Features.WindchillIntegration.EPMDocuments.Queries.GetListError;
using Application.Features.WindchillIntegration.EPMDocuments.Queries.GetSentList;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EPMDocumentsController : BaseController
	{
		[HttpGet("released/pending")]
		public async Task<ActionResult<List<GetEPMDocumentListItemDto>>> GetReleasedPending()
		{
			var result = await Mediator.Send(new GetListEPMDocumentReleasedQuery());
			return Ok(result);
		}

		[HttpGet("released/sent")]
		public async Task<ActionResult<List<GetEPMDocumentListItemDto>>> GetReleasedSent()
		{
			var result = await Mediator.Send(new GetSentEPMDocumentReleasedQuery());
			return Ok(result);
		}

		[HttpGet("released/error")]
		public async Task<ActionResult<List<GetEPMDocumentListItemDto>>> GetReleasedError()
		{
			var result = await Mediator.Send(new GetErrorEPMDocumentReleasedQuery());
			return Ok(result);
		}

		// ---------------- CANCELLED ----------------

		[HttpGet("cancelled/pending")]
		public async Task<ActionResult<List<GetEPMDocumentListItemDto>>> GetCancelledPending()
		{
			var result = await Mediator.Send(new GetListEPMDocumentCancelledQuery());
			return Ok(result);
		}

		[HttpGet("cancelled/sent")]
		public async Task<ActionResult<List<GetEPMDocumentListItemDto>>> GetCancelledSent()
		{
			var result = await Mediator.Send(new GetSentEPMDocumentCancelledQuery());
			return Ok(result);
		}

		[HttpGet("cancelled/error")]
		public async Task<ActionResult<List<GetEPMDocumentListItemDto>>> GetCancelledError()
		{
			var result = await Mediator.Send(new GetErrorEPMDocumentCancelledQuery());
			return Ok(result);
		}
	}
}