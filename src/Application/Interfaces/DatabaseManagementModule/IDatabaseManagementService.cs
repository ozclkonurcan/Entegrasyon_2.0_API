using Application.Interfaces.BaseInterfaces;
using Application.Responses;
using Domain.Entities.DatabaseManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.DatabaseManagementModule;

public interface IDatabaseManagementService:IAsyncRepository<DatabaseManagementDefinations, Guid>
{
	public Task<GetListResponse<DatabaseManagementDefinations?>> GetTablesAsync();
	public Task<string?> SetupTablels();
	public Task<DatabaseManagementDefinations?> SetupTablels(string TableName);
	public Task<DatabaseManagementDefinations?> SetupModulerTables(string TableTitle);
	public Task<DatabaseManagementDefinations?> TableControlsAsync(DatabaseManagementDefinations databaseManagementDefinations);
}


