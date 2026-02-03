using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.EPMDocumentEquivalence.Commands.Process;

public class ProcessEPMDocumentEquivalenceResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }
	public string MainNumber { get; set; }
	public string RelatedNumber { get; set; }
}