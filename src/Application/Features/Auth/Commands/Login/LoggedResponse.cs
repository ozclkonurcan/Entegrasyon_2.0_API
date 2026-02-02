using Application.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.Login;

public class LoggedResponse 
{
	public int Id { get; set; }
	public string Email { get; set; }
	public string FullName { get; set; }
	public string Role { get; set; }
	public string Token { get; set; }
	public string RefreshToken { get; set; } // Yeni eklenen
}
