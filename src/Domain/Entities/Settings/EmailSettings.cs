using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Settings;

public class EmailSettings
{
	public int Id { get; set; }
	public string Host { get; set; }
	public int Port { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }
	public bool EnableSsl { get; set; }
	public string FromEmail { get; set; }
	public bool IsActive { get; set; }
	public virtual ICollection<EmailRecipient> Recipients { get; set; }
}
