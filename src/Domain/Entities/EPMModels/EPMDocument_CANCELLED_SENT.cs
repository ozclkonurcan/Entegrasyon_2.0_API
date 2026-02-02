using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.EPMModels;

public class EPMDocument_CANCELLED_SENT
{
	public long Ent_ID { get; set; }
	public long EPMDocID { get; set; }
	public string StateDegeri { get; set; }
	public long idA3masterReference { get; set; }
	public string CadName { get; set; }
	public string name { get; set; }
	public string docNumber { get; set; }

	// Eğer Sent tablosunda tarih tutuyorsan (Genelde tutulur):
	// public DateTime? ProcessDate { get; set; } 
}
