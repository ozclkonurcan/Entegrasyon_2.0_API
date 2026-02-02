using Application.Features.Users.Rules;
using Application.Interfaces.UsersModule;
using Application.Pipelines.Logging;
using AutoMapper;
using Domain.Entities.Auth;
using Domain.Enums;
using MediatR;
using Security.Hashing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommand : IRequest<CreatedUserResponse>, ILoggableRequest
{
	public string? FullName { get; set; }
	public string Email { get; set; }
	public string Password { get; set; }
	public Role Role { get; set; }

	public string? LogMessage { get; set; }

	//public string[] Roles => new[] { Admin, Write, Add };

	public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreatedUserResponse>
	{
		private readonly IUserService _userService;
		private readonly IMapper _mapper;
		private readonly UserBusinessRules _userBusinessRules;

		public CreateUserCommandHandler(IUserService userService, IMapper mapper, UserBusinessRules userBusinessRules)
		{
			_userService = userService;
			_mapper = mapper;
			_userBusinessRules = userBusinessRules;
		}

		public async Task<CreatedUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
		{
			try
			{

			

			await _userBusinessRules.UserMailShouldNotBeExist(request.Email);

			User mappedUser = _mapper.Map<User>(request);

			byte[] passwordHash,
			passwordSalt;
			HashingHelper.CreatePasswordHash(request.Password, out passwordHash, out passwordSalt);
			mappedUser.PasswordHash = passwordHash;
			mappedUser.PasswordSalt = passwordSalt;

			User createdUser = await _userService.AddAsync(mappedUser);
			CreatedUserResponse createdUserDto = _mapper.Map<CreatedUserResponse>(createdUser);
			request.LogMessage = "Kullanici eklendi";
			return createdUserDto;
			}
			catch (Exception ex)
			{

				throw;
			}

		}
	}
}
