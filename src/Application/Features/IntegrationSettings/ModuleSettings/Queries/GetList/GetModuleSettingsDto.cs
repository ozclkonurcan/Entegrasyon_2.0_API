using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.IntegrationSettings.ModuleSettings.Queries.GetList;

    public class GetModuleSettingsDto
    {
	public int Id { get; set; }
	public string SettingsName { get; set; }
	public byte SettingsValue { get; set; }
}
