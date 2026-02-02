using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities;

public class WTPartError : RetryableEntity
{
	public int ErrorID { get; set; }
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
	public string ErrorMessage { get; set; }
	public DateTime? ErrorDate { get; set; }

	public WTPartError()
	{
		LogDate = DateTime.Now;
		EntegrasyonDurum = 0;
	}

	public WTPartError(int errorID, int logID, string parcaState, long parcaPartID, long parcaPartMasterID,
					   string parcaName, string parcaNumber, string parcaVersion, string kulAd,
					   DateTime? logDate, byte? entegrasyonDurum, string logMesaj,
					   string errorMessage, DateTime? errorDate)
	{
		ErrorID = errorID;
		LogID = logID;
		ParcaState = parcaState;
		ParcaPartID = parcaPartID;
		ParcaPartMasterID = parcaPartMasterID;
		ParcaName = parcaName;
		ParcaNumber = parcaNumber;
		ParcaVersion = parcaVersion;
		KulAd = kulAd;
		LogDate = logDate ?? DateTime.Now;
		EntegrasyonDurum = entegrasyonDurum ?? 0;
		LogMesaj = logMesaj;
		ErrorMessage = errorMessage;
		ErrorDate = errorDate;
	}
}