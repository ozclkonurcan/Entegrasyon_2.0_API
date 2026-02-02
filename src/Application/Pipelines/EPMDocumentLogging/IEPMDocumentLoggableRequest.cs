using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Pipelines.EPMDocumentLogging;

public interface IEPMDocumentLoggableRequest
{
	// Loglamada kullanılacak ortak alanlar
	string LogMessage { get; set; }
	string EPMDocID { get; set; }
	string DocNumber { get; set; }
	string CadName { get; set; }
	string StateDegeri { get; set; }
	// İstersen şunları da ekle:
	// int EntegrasyonDurum { get; set; } 
	// string ActionType { get; set; }
}
