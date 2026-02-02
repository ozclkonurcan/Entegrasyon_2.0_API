namespace WEB.Models;

public class WTUsers
{
	public string Name { get; set; }
	public string EMail { get; set; }
	public string FullName { get; set; }

	public WTUsers()
	{
	}

	public WTUsers( string name, string email, string fullName)
	{
		Name = name;
		EMail = email;
		FullName = fullName;
	}
}
