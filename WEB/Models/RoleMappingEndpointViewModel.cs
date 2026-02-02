using System.ComponentModel.DataAnnotations;

namespace WEB.Models;

public class RoleMappingEndpointViewModel
{
	[Required(ErrorMessage = "Target API zorunludur.")]
	[Display(Name = "Target API")]
	public string TargetApi { get; set; }

	[Display(Name = "Endpoint")]
	public string Endpoint { get; set; }

	[Display(Name = "Aktif Mi?")]
	public bool IsActive { get; set; }
}