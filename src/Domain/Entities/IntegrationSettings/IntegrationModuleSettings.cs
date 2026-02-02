using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.IntegrationSettings;

public class IntegrationModuleSettings
{
	public int Id { get; set; }
	public string SettingsName { get; set; }       
	public byte SettingsValue { get; set; }      
}