using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ViewboxBusiness.Common
{
	public class ViewInfo
	{
		private string _procedureCreateTempTables;

		private string _tempStatementsComplete;

		public string Name { get; set; }

		public string Description
		{
			get
			{
				if (Descriptions.Count != 0)
				{
					return Descriptions[0];
				}
				return "Keine Beschreibung vorhanden";
			}
		}

		public List<string> Descriptions { get; set; }

		public Dictionary<string, Dictionary<string, string>> ColumnDictionary { get; private set; }

		public List<string> Indizes { get; private set; }

		public List<string> Statements { get; private set; }

		public OptimizeCriterias OptimizeCriterias { get; private set; }

		public Dictionary<string, long> Tables { get; private set; }

		public List<string> Languages { get; private set; }

		public string CompleteStatement { get; set; }

		public string CompleteProcedure { get; private set; }

		public List<string> ProcedureStatements { get; private set; }

		public ObjectType ObjectType { get; set; }

		public Dictionary<string, ProcedureParameter> ParameterDictionary { get; private set; }

		public Dictionary<string, ReportParameter> ReportParameterDictionary { get; private set; }

		public List<string> ReportFilterLines { get; internal set; }

		public List<string> ReportFilterSpecialLines { get; internal set; }

		public string ProcedureName { get; set; }

		public string ProcedureCreateTempTables
		{
			get
			{
				return _procedureCreateTempTables;
			}
			set
			{
				_procedureCreateTempTables = value;
				ProcedureCreateTempTablesStatements = SplitStatements(value);
			}
		}

		public List<string> ProcedureCreateTempTablesStatements { get; set; }

		public string TempStatementsComplete
		{
			get
			{
				return _tempStatementsComplete;
			}
			set
			{
				_tempStatementsComplete = value;
				TempStatements = SplitStatements(_tempStatementsComplete);
			}
		}

		public List<string> TempStatements { get; set; }

		public ViewInfo()
		{
			Name = string.Empty;
			Tables = null;
			ColumnDictionary = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
			ParameterDictionary = new Dictionary<string, ProcedureParameter>();
			ReportParameterDictionary = new Dictionary<string, ReportParameter>();
			ReportFilterLines = new List<string>();
			ReportFilterSpecialLines = new List<string>();
			Indizes = new List<string>();
			Statements = new List<string>();
			OptimizeCriterias = new OptimizeCriterias();
			Languages = new List<string>();
			ProcedureStatements = new List<string>();
			Descriptions = new List<string>();
			ProcedureCreateTempTablesStatements = new List<string>();
			TempStatements = new List<string>();
		}

		public static List<string> SplitStatements(string sql)
		{
			List<string> result = new List<string>();
			Regex regex = new Regex(" *DELIMITER (.*)?[ \n\r]?", RegexOptions.IgnoreCase);
			List<KeyValuePair<string, int>> delimiters = new List<KeyValuePair<string, int>>
			{
				new KeyValuePair<string, int>(";", -1)
			};
			string[] lines = sql.Split(new string[1] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			int lineNumber = 0;
			string[] array = lines;
			foreach (string line in array)
			{
				Match match = regex.Match(line);
				if (match.Success && match.Groups.Count == 2)
				{
					delimiters.Add(new KeyValuePair<string, int>(match.Groups[1].Value, lineNumber));
				}
				lineNumber++;
			}
			delimiters.Add(new KeyValuePair<string, int>(";", lines.Length));
			KeyValuePair<string, int>? lastDelimiter = null;
			foreach (KeyValuePair<string, int> delimPair in delimiters)
			{
				if (!lastDelimiter.HasValue)
				{
					lastDelimiter = delimPair;
					continue;
				}
				StringBuilder currentStatement = new StringBuilder();
				for (int i = lastDelimiter.Value.Value + 1; i < delimPair.Value; i++)
				{
					currentStatement.Append(lines[i]).Append(Environment.NewLine);
				}
				Regex splitRegex = new Regex(string.Format("(?<!--.*){0}(?=(?:[^']*'[^']*')*[^']*$)", lastDelimiter.Value.Key.Replace("$", "\\$")), RegexOptions.IgnoreCase);
				result.AddRange(from statement in splitRegex.Split(currentStatement.ToString())
					where !string.IsNullOrEmpty(statement.Trim())
					select statement.Trim());
				lastDelimiter = delimPair;
			}
			return result;
		}

		public void SetCompleteProcedure(string proc)
		{
			CompleteProcedure = proc;
			ProcedureStatements = SplitStatements(proc);
		}

		public string FinalizeViewInfo()
		{
			if (Languages.Count == 0)
			{
				Languages.Add("de");
			}
			if (Descriptions.Count > Languages.Count)
			{
				return $"Es wurden {Descriptions.Count} Beschreibungen angegeben, aber nur {Languages.Count} Sprache(n) in der -- ##LANGUAGE## Sektion angegeben.";
			}
			if (Descriptions.Count == 0)
			{
				return "Es wurden keine Beschreibung angegeben, die erste Zeile nach -- ##BEGIN_VIEW## hat das Format \"Viewname;Beschreibung_Sprache1;Beschreibung_Sprache2;...\"";
			}
			return null;
		}

		public Dictionary<string, string> GetDescriptions()
		{
			Dictionary<string, string> result = new Dictionary<string, string>();
			for (int i = 0; i < Descriptions.Count; i++)
			{
				result[Languages[i]] = Descriptions[i];
			}
			return result;
		}
	}
}
