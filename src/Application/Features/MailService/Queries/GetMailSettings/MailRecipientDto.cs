using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.MailService.Queries.GetMailSettings;

public class MailRecipientDto
{
	public long Id { get; set; }
	public string EmailAddress { get; set; }
	public string DisplayName { get; set; }
}
