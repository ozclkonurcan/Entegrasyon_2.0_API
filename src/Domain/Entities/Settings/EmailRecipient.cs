using Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Settings;

public class EmailRecipient
{
	public int Id { get; set; }
	public int UserId { get; set; }
	public bool IsActive { get; set; }
	public virtual User User { get; set; }
	public virtual EmailSettings EmailSettings { get; set; }
}
