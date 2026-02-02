using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Pipelines.WTPartLogging;

public interface IWTPartLoggableRequest
{
	// Loglanacak mesaj ya da ek bilgileri buradan belirtebilirsiniz
	string LogMessage { get; }
	// İhtiyacınız olan diğer property'leri ekleyin (örneğin, Parça bilgileri)
	string ParcaState { get; }
	string ParcaPartID { get; }
	string ParcaPartMasterID { get; }
	string ParcaName { get; }
	string ParcaNumber { get; }
	string ParcaVersion { get; }
	string ActionType { get; }
	byte EntegrasyonDurum { get; }
}
