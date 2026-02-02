using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WindchillIntegration.WTPartCancelled.Commands.Process;

public class ProcessWTPartCancelledResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }

	public int LogID { get; set; }
	public string ParcaState { get; set; }
	public long? ParcaPartID { get; set; }
	public long? ParcaPartMasterID { get; set; }
	public string ParcaName { get; set; }
	public string ParcaNumber { get; set; }
	public string ParcaVersion { get; set; }
	public string KulAd { get; set; }
	public DateTime? LogDate { get; set; }
	public byte? EntegrasyonDurum { get; set; }
	public string LogMesaj { get; set; }
}
