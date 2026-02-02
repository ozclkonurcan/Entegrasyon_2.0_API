	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	namespace Domain.Entities.DatabaseManagement;

	public class TriggersDefinations
	{
		public string TriggerName { get; set; }
		public string CreateQuery { get; set; }

		public TriggersDefinations(string triggerName, string createQuery)
		{
			TriggerName = triggerName;
			CreateQuery = createQuery;
		}
	}
