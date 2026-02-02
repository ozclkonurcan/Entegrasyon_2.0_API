using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.WTParts.Queries.GetListAllAlternateLinkRemoved;

public class GetListAllAlternateLinkRemovedWTPartListItemDto
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


}
