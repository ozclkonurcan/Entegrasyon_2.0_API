using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Pipelines.Logging;

public interface ILoggableRequest
{
	string LogMessage { get; }
}
