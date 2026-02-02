using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.MailService;

public class MailRecipient : BaseEntities
{
	public long Id { get; set; }

	public long MailSettingsId { get; set; }
	public string EmailAddress { get; set; }
	public string DisplayName { get; set; }

	public bool IsActive { get; set; }

	// Navigation Property
	public virtual MailSettings MailSettings { get; set; }
}