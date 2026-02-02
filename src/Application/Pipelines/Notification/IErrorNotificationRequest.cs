using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Pipelines.Notification;

public interface IErrorNotificationRequest
{
	string OperationType { get; }
}
