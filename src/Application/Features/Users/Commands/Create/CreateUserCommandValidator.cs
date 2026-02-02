using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Users.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
	public CreateUserCommandValidator()
	{
		RuleFor(c => c.Email).NotEmpty().EmailAddress();
		//RuleFor(c => c.FullName).NotEmpty().MinimumLength(2);


}
}
