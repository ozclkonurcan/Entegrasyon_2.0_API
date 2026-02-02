using Domain.Entities.IntegrationSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.IntegrationSettings;

public interface IIntegrationSettingsService
{
	// RoleMapping CRUD işlemleri
	Task<IEnumerable<RoleMapping>> GetRoleMappingsAsync();
	Task<RoleMapping> GetRoleMappingByIdAsync(int id);
	Task<RoleMapping> CreateRoleMappingAsync(RoleMapping roleMapping);
	Task<RoleMapping> UpdateRoleMappingAsync(RoleMapping roleMapping);
	Task<bool> DeleteRoleMappingAsync(int id);

	// IntegrationModuleSettings CRUD işlemleri
	Task<IEnumerable<IntegrationModuleSettings>> GetIntegrationModuleSettingsAsync();
	Task<IntegrationModuleSettings> GetIntegrationModuleSettingByIdAsync(int id);
	Task<IntegrationModuleSettings> GetModuleSettingsAsync(string SettingsName);
	Task<IntegrationModuleSettings> CreateIntegrationModuleSettingAsync(IntegrationModuleSettings settings);
	Task<IntegrationModuleSettings> UpdateIntegrationModuleSettingAsync(IntegrationModuleSettings settings);
	Task<bool> DeleteIntegrationModuleSettingAsync(int id);


	// Yeni: RoleProcessTag CRUD işlemleri
	Task<IEnumerable<RoleProcessTag>> GetRoleProcessTagsAsync();
	Task<RoleProcessTag> GetRoleProcessTagByIdAsync(int id);
	Task<RoleProcessTag> CreateRoleProcessTagAsync(RoleProcessTag roleProcessTag);
	Task<RoleProcessTag> UpdateRoleProcessTagAsync(RoleProcessTag roleProcessTag);
	Task<bool> DeleteRoleProcessTagAsync(int id);
	Task<RoleMapping> GetRoleMappingByProcessTagIdAsync(int processTagId);



}
