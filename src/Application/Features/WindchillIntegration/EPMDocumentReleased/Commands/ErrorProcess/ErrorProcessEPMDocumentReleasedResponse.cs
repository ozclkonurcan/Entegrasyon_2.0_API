using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocumentReleased.Commands.ErrorProcess;

public class ErrorProcessEPMDocumentReleasedResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }

	public long Ent_ID { get; set; }
	public long EPMDocID { get; set; }
	public string StateDegeri { get; set; }
	public long idA3masterReference { get; set; }
	public string CadName { get; set; }
	public string name { get; set; }
	public string docNumber { get; set; }
}
