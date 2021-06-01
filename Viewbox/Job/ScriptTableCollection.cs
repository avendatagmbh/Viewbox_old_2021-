using System.Collections.Generic;
using System.Text;

namespace Viewbox.Job
{
	internal class ScriptTableCollection : Dictionary<string, ScriptTable>
	{
		public void Add(ScriptTable table)
		{
			if (!ContainsKey(table.Name))
			{
				Add(table.Name, table);
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach (ScriptTable scriptTable in base.Values)
			{
				sb.Append(scriptTable.ToString());
			}
			return sb.ToString();
		}
	}
}
