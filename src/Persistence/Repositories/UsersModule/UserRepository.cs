using Domain.Entities.Auth;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.UsersModule;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Application.Paging;
using Microsoft.EntityFrameworkCore;
using Security.Entities;
using System.Linq.Dynamic.Core;

namespace Persistence.Repositories.UsersModule;

public class UserRepository : IUserService
{
	private readonly BaseDbContexts _baseDbContexts;

	public UserRepository(BaseDbContexts baseDbContexts)
	{
		_baseDbContexts = baseDbContexts;
	}

	public async Task<User> AddAsync(User entity)
	{
		await _baseDbContexts.AddAsync(entity);
		entity.CreatedDate = DateTime.UtcNow;
		await _baseDbContexts.SaveChangesAsync();
		return entity;
	}

	public Task<ICollection<User>> AddRangeAsync(ICollection<User> entities)
	{
		throw new NotImplementedException();
	}

	public async Task<bool> AnyAsync(Expression<Func<User, bool>>? predicate = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
	{
		IQueryable<User> queryble = Query();
		if (!enableTracking)
			queryble = queryble.AsNoTracking();
		if (withDeleted)
			queryble = queryble.IgnoreQueryFilters();
		if (predicate != null)
			queryble = queryble.Where(predicate);
		return await queryble.AnyAsync(cancellationToken);
	}

	public Task<User> DeleteAsync(User entity, bool permanent = false)
	{
		throw new NotImplementedException();
	}

	public Task<ICollection<User>> DeleteRangeAsync(ICollection<User> entities, bool permanent = false)
	{
		throw new NotImplementedException();
	}

	public Task<User?> GetAsync(Expression<Func<User, bool>> predicate, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public async Task<User?> GetByEmail(string email)
	{
		try
		{

		return await _baseDbContexts.Set<User>().FirstOrDefaultAsync(x => x.Email == email);
		}
		catch (Exception ex)
		{

			throw;
		}
	}

	public Task<User> GetById(int id)
	{
		throw new NotImplementedException();
	}

	public Task<Paginate<User>> GetListAsync(Expression<Func<User, bool>>? predicate = null, Func<IQueryable<User>, IOrderedQueryable<User>>? orderBy = null, Func<IQueryable<User>, IIncludableQueryable<User, object>>? include = null, int index = 0, int size = 10, bool withDeleted = false, bool enableTracking = true, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}

	public IQueryable<User> Query()
	{
		return _baseDbContexts.Set<User>();
	}

	public Task<User> Update(User user)
	{
		throw new NotImplementedException();
	}

	public Task<User> UpdateAsync(User entity)
	{
		throw new NotImplementedException();
	}

	public Task<ICollection<User>> UpdateRangeAsync(ICollection<User> entities)
	{
		throw new NotImplementedException();
	}
}
