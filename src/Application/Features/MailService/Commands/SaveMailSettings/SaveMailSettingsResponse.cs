using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.MailService.Commands.SaveMailSettings;
public class SaveMailSettingsResponse
{
	public bool Success { get; set; }
	public string Message { get; set; }
	public long MailSettingsId { get; set; }
}
