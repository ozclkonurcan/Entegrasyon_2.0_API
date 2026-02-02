using Application.Features.MailService.Commands.SaveMailSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.MailService.Queries.GetMailSettings;

public class GetMailSettingsDto
{
	public bool Success { get; set; }
	public string Message { get; set; }
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

	public List<MailRecipientDto> Recipients { get; set; } = new List<MailRecipientDto>();
}
