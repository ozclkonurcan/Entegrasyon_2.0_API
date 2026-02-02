using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.EPMModels;

public class EPMDocumentMaster
{


	public long idA2A2 { get; set; }
	public string? CADName { get; set; }
	public string? name { get; set; }
	public string? documentNumber { get; set; }

	public EPMDocumentMaster(long idA2A2, string? cADName, string? name, string? documentNumber)
	{
		this.idA2A2 = idA2A2;
		CADName = cADName;
		this.name = name;
		this.documentNumber = documentNumber;
	}
}
