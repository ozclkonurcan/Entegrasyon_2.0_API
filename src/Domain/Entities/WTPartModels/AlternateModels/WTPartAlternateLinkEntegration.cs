using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.WTPartModels.AlternateModels;

public class WTPartAlternateLinkEntegration
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

	public WTPartAlternateLinkEntegration()
	{
		LogDate = DateTime.Now;
		EntegrasyonDurum = 0;
	}

	public WTPartAlternateLinkEntegration(int logID, string anaParcaState, long anaParcaPartID, long anaParcaPartMasterID,
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



#region Equivalence


//public class EPMDocumentEquivalenceLink
//{
//	public int LogID { get; set; }
//	public string AnaDokumanState { get; set; }
//	public long AnaDokumanID { get; set; }
//	public long AnaDokumanMasterID { get; set; }
//	public string AnaDokumanName { get; set; }
//	public string AnaDokumanNumber { get; set; }
//	public string AnaDokumanVersion { get; set; }
//	public string EsdegerDokumanState { get; set; }
//	public long EsdegerDokumanID { get; set; }
//	public long EsdegerDokumanMasterID { get; set; }
//	public string EsdegerDokumanName { get; set; }
//	public string EsdegerDokumanNumber { get; set; }
//	public string EsdegerDokumanVersion { get; set; }
//	public string KulAd { get; set; }
//	public string LogMesaj { get; set; }
//	public DateTime? LogDate { get; set; }
//	public byte? EntegrasyonDurum { get; set; }
//	public DateTime? EntegrasyonTarihi { get; set; }
//	public string EntegrasyonHataMesaji { get; set; }

//	public EPMDocumentEquivalenceLink()
//	{
//		LogDate = DateTime.Now;
//		EntegrasyonDurum = 0;
//	}

//	public EPMDocumentEquivalenceLink(int logID, string anaDokumanState, long anaDokumanID, long anaDokumanMasterID,
//		string anaDokumanName, string anaDokumanNumber, string anaDokumanVersion,
//		string esdegerDokumanState, long esdegerDokumanID, long esdegerDokumanMasterID,
//		string esdegerDokumanName, string esdegerDokumanNumber, string esdegerDokumanVersion,
//		string kulAd, string logMesaj, DateTime? logDate, byte? entegrasyonDurum) : this()
//	{
//		LogID = logID;
//		AnaDokumanState = anaDokumanState;
//		AnaDokumanID = anaDokumanID;
//		AnaDokumanMasterID = anaDokumanMasterID;
//		AnaDokumanName = anaDokumanName;
//		AnaDokumanNumber = anaDokumanNumber;
//		AnaDokumanVersion = anaDokumanVersion;
//		EsdegerDokumanState = esdegerDokumanState;
//		EsdegerDokumanID = esdegerDokumanID;
//		EsdegerDokumanMasterID = esdegerDokumanMasterID;
//		EsdegerDokumanName = esdegerDokumanName;
//		EsdegerDokumanNumber = esdegerDokumanNumber;
//		EsdegerDokumanVersion = esdegerDokumanVersion;
//		KulAd = kulAd;
//		LogMesaj = logMesaj;
//		LogDate = logDate;
//		EntegrasyonDurum = entegrasyonDurum;
//	}
//}

//public class EPMDocumentEquivalenceLinkRemoved
//{
//	public int LogID { get; set; }
//	public string AnaDokumanState { get; set; }
//	public long AnaDokumanID { get; set; }
//	public long AnaDokumanMasterID { get; set; }
//	public string AnaDokumanName { get; set; }
//	public string AnaDokumanNumber { get; set; }
//	public string AnaDokumanVersion { get; set; }
//	public string EsdegerDokumanState { get; set; }
//	public long EsdegerDokumanID { get; set; }
//	public long EsdegerDokumanMasterID { get; set; }
//	public string EsdegerDokumanName { get; set; }
//	public string EsdegerDokumanNumber { get; set; }
//	public string EsdegerDokumanVersion { get; set; }
//	public string KulAd { get; set; }
//	public string LogMesaj { get; set; }
//	public DateTime? LogDate { get; set; }
//	public byte? EntegrasyonDurum { get; set; }
//	public DateTime? EntegrasyonTarihi { get; set; }
//	public string EntegrasyonHataMesaji { get; set; }

//	public EPMDocumentEquivalenceLinkRemoved()
//	{
//		LogDate = DateTime.Now;
//		EntegrasyonDurum = 0;
//	}

//	public EPMDocumentEquivalenceLinkRemoved(int logID, string anaDokumanState, long anaDokumanID, long anaDokumanMasterID,
//		string anaDokumanName, string anaDokumanNumber, string anaDokumanVersion,
//		string esdegerDokumanState, long esdegerDokumanID, long esdegerDokumanMasterID,
//		string esdegerDokumanName, string esdegerDokumanNumber, string esdegerDokumanVersion,
//		string kulAd, string logMesaj, DateTime? logDate, byte? entegrasyonDurum) : this()
//	{
//		LogID = logID;
//		AnaDokumanState = anaDokumanState;
//		AnaDokumanID = anaDokumanID;
//		AnaDokumanMasterID = anaDokumanMasterID;
//		AnaDokumanName = anaDokumanName;
//		AnaDokumanNumber = anaDokumanNumber;
//		AnaDokumanVersion = anaDokumanVersion;
//		EsdegerDokumanState = esdegerDokumanState;
//		EsdegerDokumanID = esdegerDokumanID;
//		EsdegerDokumanMasterID = esdegerDokumanMasterID;
//		EsdegerDokumanName = esdegerDokumanName;
//		EsdegerDokumanNumber = esdegerDokumanNumber;
//		EsdegerDokumanVersion = esdegerDokumanVersion;
//		KulAd = kulAd;
//		LogMesaj = logMesaj;
//		LogDate = logDate;
//		EntegrasyonDurum = entegrasyonDurum;
//	}
//}
#endregion
