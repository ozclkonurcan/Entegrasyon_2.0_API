using System.Text.Json.Serialization;

namespace WEB.Models;

public class RoleAttributeViewModel
{
	public int Id { get; set; }
	public int RoleMappingId { get; set; }
	public string AttributeName { get; set; }
	public bool IsSelected { get; set; }
}

