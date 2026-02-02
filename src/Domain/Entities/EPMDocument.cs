using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;


public class EPMDocument
{
	public long Ent_ID { get; set; }
	public long EPMDocID { get; set; }
	public string StateDegeri { get; set; }
	public long idA3masterReference { get; set; }
	public string CadName { get; set; }
	public string name { get; set; }
	public string docNumber { get; set; }
}
//public class EPMDocument
//{


//	public long idA2A2 { get; set; }
//	public string statecheckoutInfo { get; set; }
	
//	public string? statestate { get; set; }
//	public string versionIdA2versionInfo { get; set; }
//	public int versionLevelA2versionInfo	 { get; set; }
//	public byte latestiterationInfo { get; set; }
//	public long idA3masterReference { get; set; }
//	public EPMDocument(long idA2A2, string statecheckoutInfo, string? statestate, string versionIdA2versionInfo, int versionLevelA2versionInfo, byte latestiterationInfo, long idA3masterReference)
//	{
//		this.idA2A2 = idA2A2;
//		this.statecheckoutInfo = statecheckoutInfo;
//		this.statestate = statestate;
//		this.versionIdA2versionInfo = versionIdA2versionInfo;
//		this.versionLevelA2versionInfo = versionLevelA2versionInfo;
//		this.latestiterationInfo = latestiterationInfo;
//		this.idA3masterReference = idA3masterReference;
//	}
//}
