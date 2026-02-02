namespace Domain.Entities;

public class EPMDocument_CANCELLED
{


	public long Ent_ID { get; set; }
	public long EPMDocID { get; set; }
	public string StateDegeri { get; set; }
	public long idA3masterReference { get; set; }
	public string CadName { get; set; }
	public string name { get; set; }
	public string docNumber { get; set; }

	public EPMDocument_CANCELLED(long ent_ID, long ePMDocID, string stateDegeri, long idA3masterReference, string cadName, string name, string docNumber)
	{
		Ent_ID = ent_ID;
		EPMDocID = ePMDocID;
		StateDegeri = stateDegeri;
		this.idA3masterReference = idA3masterReference;
		CadName = cadName;
		this.name = name;
		this.docNumber = docNumber;
	}
}