using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WEB.Models;


public class RoleMappingViewModel
{
	[JsonPropertyName("id")]
	public int? Id { get; set; }  // Create için boş olabilir.

	[Display(Name = "Rol Adı")]
	[Required(ErrorMessage = "Rol adı zorunludur.")]
	public string RoleName { get; set; }

	[Display(Name = "Kaynak API")]
	public string SourceApi { get; set; }

	[Display(Name = "Aktif Mi?")]
	public bool IsActive { get; set; }

	[Display(Name = "Process Tag")]
	[Required(ErrorMessage = "Process Tag seçimi zorunludur.")]
	public int ProcessTagId { get; set; }

	// Listeleme için ek alan
	public string? ProcessTagName { get; set; }

	public List<RoleMappingEndpointViewModel> Endpoints { get; set; } = new List<RoleMappingEndpointViewModel>();


	public List<RoleAttributeViewModel> WindchillAttributes { get; set; } = new List<RoleAttributeViewModel>();

}
