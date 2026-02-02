using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Notification;

public class NotificationHistory
{
	public int Id { get; set; }
	public int ErrorNotificationId { get; set; }
	public string Recipients { get; set; }
	public string Subject { get; set; }
	public string Content { get; set; }
	public DateTime SentTime { get; set; }
	public bool IsSuccess { get; set; }
	public virtual ErrorNotification ErrorNotification { get; set; }
}
