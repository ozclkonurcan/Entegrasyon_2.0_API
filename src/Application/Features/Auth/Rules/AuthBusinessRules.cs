using Application.Features.Users.Constants;
using Application.Interfaces.UsersModule;
using CrossCuttingConcerns.ExceptionHandling.Types;
using Domain.Entities.Auth;
using Security.Enums;
using Security.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Auth.Rules;

public class AuthBusinessRules : BaseBusinessRules
{
	private readonly IUserService _userService;

	public AuthBusinessRules(IUserService userService)
	{
		_userService = _userService;
	}



}
