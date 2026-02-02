using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MailService;

public class MailSettings : BaseEntities
{
	public long Id { get; set; }

	public string SmtpServer { get; set; }
	public int SmtpPort { get; set; }
	public string SmtpUsername { get; set; }
	public string SmtpPassword { get; set; }
	public bool EnableSsl { get; set; }
	public string FromEmail { get; set; }
	public string FromDisplayName { get; set; }

	public bool SendOnError { get; set; }
	public bool SendOnSuccess { get; set; }
	public bool SendOnFinalFailure { get; set; }

	public bool IsActive { get; set; }

	// Navigation Property
	public virtual ICollection<MailRecipient> MailRecipients { get; set; } = new List<MailRecipient>();
}