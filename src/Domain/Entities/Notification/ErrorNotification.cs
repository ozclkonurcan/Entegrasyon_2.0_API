using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Notification;

public class ErrorNotification
{
	public int Id { get; set; }
	public string ErrorKey { get; set; }
	public string ErrorType { get; set; }
	public string ErrorMessage { get; set; }
	public string OperationType { get; set; }
	public int ErrorCount { get; set; }
	public DateTime FirstOccurrence { get; set; }
	public DateTime LastOccurrence { get; set; }
	public DateTime? LastNotificationTime { get; set; }
	public bool IsActive { get; set; }
	public DateTime CreatedDate { get; set; }
	public DateTime UpdatedDate { get; set; }
}
