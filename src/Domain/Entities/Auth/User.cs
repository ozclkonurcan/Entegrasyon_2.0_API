using Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities.Auth;

public class User : BaseEntities
{
	[Key]
	public int Id { get; set; }
	public int? UserId { get; set; }
	public string FullName { get; set; }
	public string Email { get; set; }
	public byte[] PasswordSalt { get; set; }
	public byte[] PasswordHash { get; set; }
	public string? AuthenticatorType { get; set; }
	public Role Role { get; set; }

	public User()
	{
		
	}

	public User(int userId, string fullName, string email, byte[] passwordSalt, byte[] passwordHash, string authenticatorType, Role role)
	{
		Email = email;
		UserId = userId;
		FullName = fullName;
		PasswordSalt = passwordSalt;
		PasswordHash = passwordHash;
		AuthenticatorType = authenticatorType;
		Role = role;
	}
}
