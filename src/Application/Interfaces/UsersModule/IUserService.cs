using Application.Interfaces.BaseInterfaces;
using Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.UsersModule
{
	public interface IUserService : IAsyncRepository<User,Guid>
	{
		public Task<User?> GetByEmail(string email);
		public Task<User> GetById(int id);
		public Task<User> Update(User user);
	}
}
