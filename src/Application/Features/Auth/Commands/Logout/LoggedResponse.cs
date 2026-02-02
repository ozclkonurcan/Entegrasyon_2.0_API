using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Commands.Logout;
public class LoggedResponse
{
	public int Id { get; set; }
	public string Email { get; set; }
	public string FullName { get; set; }
	public string Token { get; set; }
}