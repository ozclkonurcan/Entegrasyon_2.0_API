using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.EPMDocuments.Queries.GetListAll
{
	public class GetListAllEPMDocumentListItemDto
	{
		public long Ent_ID { get; set; }
		public long EPMDocID { get; set; }
		public string StateDegeri { get; set; }
		public long idA3masterReference { get; set; }
		public string CadName { get; set; }
		public string name { get; set; }
		public string docNumber { get; set; }

		public string Id { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime UpdatedDate { get; set; }
		public DateTime DeletedDate { get; set; }
	}
}
