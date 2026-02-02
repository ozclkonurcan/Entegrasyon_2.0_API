using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.WTPartModels.AlternateRemovedModels;

public class WTPartAlternateLinkRemovedEntegration
{
	public int LogID { get; set; }
	public string AnaParcaState { get; set; }
	public long AnaParcaPartID { get; set; }
	public long AnaParcaPartMasterID { get; set; }
	public string AnaParcaName { get; set; }
	public string AnaParcaNumber { get; set; }
	public string AnaParcaVersion { get; set; }
	public string MuadilParcaState { get; set; }
	public long MuadilParcaPartID { get; set; }
	public long MuadilParcaMasterID { get; set; }
	public string MuadilParcaName { get; set; }
	public string MuadilParcaNumber { get; set; }
	public string MuadilParcaVersion { get; set; }
	public string KulAd { get; set; }
	public string LogMesaj { get; set; }
	public DateTime? LogDate { get; set; }
	public byte? EntegrasyonDurum { get; set; }
	//public DateTime? EntegrasyonTarihi { get; set; }
	//public string EntegrasyonHataMesaji { get; set; }

	public WTPartAlternateLinkRemovedEntegration()
	{
		LogDate = DateTime.Now;
		EntegrasyonDurum = 0;
	}

	public WTPartAlternateLinkRemovedEntegration(int logID, string anaParcaState, long anaParcaPartID, long anaParcaPartMasterID,
		string anaParcaName, string anaParcaNumber, string anaParcaVersion,
		string muadilParcaState, long muadilParcaPartID, long muadilParcaMasterID,
		string muadilParcaName, string muadilParcaNumber, string muadilParcaVersion,
		string kulAd, string logMesaj, DateTime? logDate, byte? entegrasyonDurum) : this()
	{
		LogID = logID;
		AnaParcaState = anaParcaState;
		AnaParcaPartID = anaParcaPartID;
		AnaParcaPartMasterID = anaParcaPartMasterID;
		AnaParcaName = anaParcaName;
		AnaParcaNumber = anaParcaNumber;
		AnaParcaVersion = anaParcaVersion;
		MuadilParcaState = muadilParcaState;
		MuadilParcaPartID = muadilParcaPartID;
		MuadilParcaMasterID = muadilParcaMasterID;
		MuadilParcaName = muadilParcaName;
		MuadilParcaNumber = muadilParcaNumber;
		MuadilParcaVersion = muadilParcaVersion;
		KulAd = kulAd;
		LogMesaj = logMesaj;
		LogDate = logDate;
		EntegrasyonDurum = entegrasyonDurum;
	}
}
