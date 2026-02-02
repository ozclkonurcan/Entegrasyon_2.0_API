using Application.Features.Auth.Constants;
using Application.Features.Users.Constants;
using Application.Interfaces.UsersModule;
using CrossCuttingConcerns.ExceptionHandling.Types;
using Domain.Entities.Auth;
using Security.Hashing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Rules;


public class UserBusinessRules : BaseBusinessRules
{
	private readonly IUserService _userService;

	public UserBusinessRules(IUserService userService)
	{
		_userService = userService;
	}


	public async Task UserIdShouldExistWhenSelected(int id)
	{
		User? result = await _userService.GetAsync(predicate: b => b.Id == id, enableTracking: false);
		if (result == null)
			throw new BusinessException(UsersMessages.UserDontExists);
	}

	public Task UserShouldBeExist(User? user)
	{
		if (user is null)
			throw new BusinessException(UsersMessages.UserDontExists);
		return Task.CompletedTask;
	}

	public Task UserPasswordShouldBeMatch(User user, string password)
	{
		if (!HashingHelper.VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
			throw new BusinessException(UsersMessages.PasswordDontMatch);
		return Task.CompletedTask;
	}

	public async Task UserMailShouldNotBeExist(string email)
	{
		bool doesExists = await _userService.AnyAsync(predicate: u => u.Email == email, enableTracking: false);
		if (doesExists)
			throw new BusinessException(UsersMessages.UserMailAlreadyExists);
	}


}

