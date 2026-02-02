using Application.Interfaces.IntegrationSettings;
using Domain.Entities.IntegrationSettings;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories.IntegrationSettings;

public class IntegrationSettingsRepository : IIntegrationSettingsService
{
	private readonly BaseDbContexts _context;

	public IntegrationSettingsRepository(BaseDbContexts context)
	{
		_context = context;
	}

	// RoleMapping CRUD
	public async Task<IEnumerable<RoleMapping>> GetRoleMappingsAsync()
	{
		//return await _context.RoleMappings.ToListAsync();
		//return await _context.RoleMappings.Include(r => r.Endpoints).ToListAsync();
		return await _context.RoleMappings
			.Include(r => r.Endpoints)
			.Include(r => r.WindchillAttributes)
			.ToListAsync();
	}

	public async Task<RoleMapping> GetRoleMappingByIdAsync(int id)
	{
		//return await _context.RoleMappings.FirstOrDefaultAsync(r => r.Id == id);
		//return await _context.RoleMappings.Include(r => r.Endpoints).FirstOrDefaultAsync(r => r.Id == id);
		return await _context.RoleMappings
		.Include(r => r.Endpoints)
		.Include(r => r.WindchillAttributes)
		.FirstOrDefaultAsync(r => r.Id == id);
	}

	public async Task<RoleMapping> CreateRoleMappingAsync(RoleMapping roleMapping)
	{
		_context.RoleMappings.Add(roleMapping);
		await _context.SaveChangesAsync();
		return roleMapping;
	}

	public async Task<RoleMapping> UpdateRoleMappingAsync(RoleMapping roleMapping)
	{
		_context.RoleMappings.Update(roleMapping);
		await _context.SaveChangesAsync();
		return roleMapping;
	}

	public async Task<bool> DeleteRoleMappingAsync(int id)
	{
		var entity = await _context.RoleMappings.FindAsync(id);
		if (entity == null)
			return false;
		_context.RoleMappings.Remove(entity);
		await _context.SaveChangesAsync();
		return true;
	}



	// IntegrationModuleSettings CRUD
	public async Task<IEnumerable<IntegrationModuleSettings>> GetIntegrationModuleSettingsAsync()
	{
		return await _context.IntegrationModuleSettings.ToListAsync();
	}

	public async Task<IntegrationModuleSettings> GetIntegrationModuleSettingByIdAsync(int id)
	{
		return await _context.IntegrationModuleSettings.FindAsync(id);
	}

	public async Task<IntegrationModuleSettings> CreateIntegrationModuleSettingAsync(IntegrationModuleSettings settings)
	{
		_context.IntegrationModuleSettings.Add(settings);
		await _context.SaveChangesAsync();
		return settings;
	}

	public async Task<IntegrationModuleSettings> UpdateIntegrationModuleSettingAsync(IntegrationModuleSettings settings)
	{
		_context.IntegrationModuleSettings.Update(settings);
		await _context.SaveChangesAsync();
		return settings;
	}

	public async Task<bool> DeleteIntegrationModuleSettingAsync(int id)
	{
		var entity = await _context.IntegrationModuleSettings.FindAsync(id);
		if (entity == null)
			return false;
		_context.IntegrationModuleSettings.Remove(entity);
		await _context.SaveChangesAsync();
		return true;
	}

	public async Task<IntegrationModuleSettings> GetModuleSettingsAsync(string settingsName)
	{
		return await _context.IntegrationModuleSettings
							 .FirstOrDefaultAsync(s => s.SettingsName == settingsName);
	}



	public async Task<IEnumerable<RoleProcessTag>> GetRoleProcessTagsAsync()
	{
		return await _context.Set<RoleProcessTag>().ToListAsync();
	}

	public async Task<RoleProcessTag> GetRoleProcessTagByIdAsync(int id)
	{
		return await _context.Set<RoleProcessTag>().FindAsync(id);
	}

	public async Task<RoleMapping> GetRoleMappingByProcessTagIdAsync(int processTagId)
	{
		return await _context.RoleMappings
			.Include(r => r.Endpoints) // Bağlı endpoints'leri de çekiyoruz
			.Include(r => r.WindchillAttributes) 
			.FirstOrDefaultAsync(r => r.ProcessTagID == processTagId && r.IsActive == true);
	}

	public async Task<RoleProcessTag> CreateRoleProcessTagAsync(RoleProcessTag roleProcessTag)
	{
		_context.Set<RoleProcessTag>().Add(roleProcessTag);
		await _context.SaveChangesAsync();
		return roleProcessTag;
	}

	public async Task<RoleProcessTag> UpdateRoleProcessTagAsync(RoleProcessTag roleProcessTag)
	{
		_context.Set<RoleProcessTag>().Update(roleProcessTag);
		await _context.SaveChangesAsync();
		return roleProcessTag;
	}

	public async Task<bool> DeleteRoleProcessTagAsync(int id)
	{
		var entity = await _context.Set<RoleProcessTag>().FindAsync(id);
		if (entity == null)
			return false;
		_context.Set<RoleProcessTag>().Remove(entity);
		await _context.SaveChangesAsync();
		return true;
	}


}
