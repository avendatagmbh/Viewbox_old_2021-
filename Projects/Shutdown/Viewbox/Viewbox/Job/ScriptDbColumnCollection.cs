using System.Collections.Generic;

namespace Viewbox.Job
{
	internal class ScriptDbColumnCollection : Dictionary<string, ScriptDbColumn>
	{
		public void Add(ScriptDbColumn column)
		{
			if (!ContainsKey(column.Name))
			{
				Add(column.Name, column);
			}
		}
	}
}
