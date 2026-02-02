using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class WTPart
{


	public int LogID { get; set; }
	public string ParcaState { get; set; }
	public long ParcaPartID { get; set; }
	public long ParcaPartMasterID { get; set; }
	public string ParcaName { get; set; }
	public string ParcaNumber { get; set; }
	public string ParcaVersion { get; set; }
	public string KulAd { get; set; }
	public DateTime? LogDate { get; set; }
	public byte? EntegrasyonDurum { get; set; }
	public string LogMesaj { get; set; }


	public WTPart()
	{

		LogDate = DateTime.Now;
		EntegrasyonDurum = 0;
	}

	public WTPart(int logID, string parcaState, long parcaPartID, long parcaPartMasterID, string parcaName, string parcaNumber, string parcaVersion, string kulAd, DateTime? logDate, byte? entegrasyonDurum, string logMesaj) : this()
	{
		LogID = logID;
		ParcaState = parcaState;
		ParcaPartID = parcaPartID;
		ParcaPartMasterID = parcaPartMasterID;
		ParcaName = parcaName;
		ParcaNumber = parcaNumber;
		ParcaVersion = parcaVersion;
		KulAd = kulAd;
		LogDate = logDate;
		EntegrasyonDurum = entegrasyonDurum;
		LogMesaj = logMesaj;
	}
}








//	[Key]
//public string? ID { get; set; }
//public string? Number { get; set; }
//public string? Name { get; set; }
//public string? Description { get; set; }
//public State? State { get; set; }
////public string? MuhasebeKodu { get; set; } = "0000000";
//public ProjeAdi? ProjeAdi { get; set; }
//public string? ProjeKodu { get; set; }
//public MuhasebeAdi? MuhasebeAdi { get; set; }
//public string? MuhasebeKodu { get; set; }
//public BirimAdi? BirimAdi { get; set; }
//public string? BirimKodu { get; set; }

//public PlanlamaTipiAdi? PlanlamaTipiAdi { get; set; }
//public string? PlanlamaTipiKodu { get; set; } = "P";
//public string? Fai { get; set; }
//public string? PLM { get; set; } = "E";
//public CLASSIFICATION? CLASSIFICATION { get; set; }
//public string? EntegrasyonDurumu { get; set; }
//public string? EntegrasyonTarihi { get; set; }
//public List<Alternates>? Alternates { get; set; }


//public DateTime? CreatedOn { get; set; }
//public DateTime? LastModified { get; set; }
//public string? Version { get; set; }
//public string? VersionID { get; set; }