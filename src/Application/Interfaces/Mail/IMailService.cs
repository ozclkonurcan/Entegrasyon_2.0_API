using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Mail;
public interface IMailService
{
	Task SendErrorMailAsync(string entityType, string entityNumber, string entityName, string errorMessage, long? entityId = null);
	Task SendSuccessMailAsync(string entityType, string entityNumber, string entityName, string successMessage, long? entityId = null);
	Task SendCustomMailAsync(string subject, string body, string mailType = "Custom", string entityType = null, long? entityId = null);
}