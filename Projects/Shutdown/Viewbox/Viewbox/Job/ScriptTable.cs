using System;
using System.Collections.Generic;
using System.Text;

namespace Viewbox.Job
{
	internal class ScriptTable : ICloneable
	{
		private ScriptDbColumnCollection _scriptColumns = new ScriptDbColumnCollection();

		private ScriptTableCollection _scriptTables = new ScriptTableCollection();

		public string Name { get; set; }

		public string Alias { get; set; }

		public ScriptDbColumnCollection ScriptColumns
		{
			get
			{
				return _scriptColumns;
			}
			set
			{
				_scriptColumns = value;
			}
		}

		public ScriptTableCollection ScriptTables
		{
			get
			{
				return _scriptTables;
			}
			set
			{
				_scriptTables = value;
			}
		}

		public object Clone()
		{
			return new ScriptTable
			{
				Name = Name,
				ScriptColumns = ScriptColumns,
				ScriptTables = ScriptTables
			};
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("TableName : " + Name);
			foreach (KeyValuePair<string, ScriptDbColumn> scriptColumn in ScriptColumns)
			{
				sb.AppendLine("-Column : " + scriptColumn.Value);
			}
			foreach (KeyValuePair<string, ScriptTable> scriptTable in ScriptTables)
			{
				sb.AppendLine("-FromTable : " + scriptTable.Key + ", Alias : " + scriptTable.Value.Alias);
			}
			return sb.ToString();
		}
	}
}
