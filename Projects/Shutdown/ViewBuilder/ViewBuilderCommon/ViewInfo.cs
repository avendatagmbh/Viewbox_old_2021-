using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ViewBuilderCommon
{
    /// <summary>
    ///   This class represents a parsed view script.
    /// </summary>
    public class ViewInfo
    {
        public ViewInfo()
        {
            Name = string.Empty;
            //this.Description = string.Empty;
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

        /// <summary>
        ///   Gets or sets the name.
        /// </summary>
        /// <value> The name. </value>
        public string Name { get; set; }

        /// <summary>
        ///   Gets or sets the description.
        /// </summary>
        /// <value> The description. </value>
        public string Description
        {
            get { return Descriptions.Count == 0 ? "Keine Beschreibung vorhanden" : Descriptions[0]; }
        }

        public List<string> Descriptions { get; set; }

        /// <summary>
        ///   Gets or sets the dictionary for the column descriptions.
        /// </summary>
        /// <value> The column dict. </value>
        public Dictionary<string, Dictionary<string, string>> ColumnDictionary { get; private set; }

        /// <summary>
        ///   Gets or sets the indizes.
        /// </summary>
        /// <value> The indizes. </value>
        public List<string> Indizes { get; private set; }

        /// <summary>
        ///   Gets or sets the statements.
        /// </summary>
        /// <value> The statements. </value>
        public List<string> Statements { get; private set; }

        /// <summary>
        ///   Gets or sets the optimize criterias.
        /// </summary>
        /// <value> The optimize criterias. </value>
        public OptimizeCriterias OptimizeCriterias { get; private set; }

        /// <summary>
        ///   Gets or sets the tables.
        /// </summary>
        /// <value> The tables. </value>
        public Dictionary<string, long> Tables { get; private set; }

        public List<string> Languages { get; private set; }
        public string CompleteStatement { get; set; }
        public string CompleteProcedure { get; private set; }
        public List<string> ProcedureStatements { get; private set; }

        public ObjectType ObjectType { get; set; }
        //Dictionary from parameter name to procedure parameter
        public Dictionary<string, ProcedureParameter> ParameterDictionary { get; private set; }
        //Dictionary from parameter name to report parameter
        public Dictionary<string, ReportParameter> ReportParameterDictionary { get; private set; }
        //The report filter (could be in multiple lines)
        public List<string> ReportFilterLines { get; internal set; }
        public List<string> ReportFilterSpecialLines { get; internal set; }
        public string ProcedureName { get; set; }

        #region ProcedureCreateTempTables

        private string _procedureCreateTempTables;

        public string ProcedureCreateTempTables
        {
            get { return _procedureCreateTempTables; }
            set
            {
                _procedureCreateTempTables = value;
                ProcedureCreateTempTablesStatements = SplitStatements(value);
            }
        }

        public List<string> ProcedureCreateTempTablesStatements { get; set; }

        #endregion ProcedureCreateTempTables

        #region TempStatementsComplete

        private string _tempStatementsComplete;

        public string TempStatementsComplete
        {
            get { return _tempStatementsComplete; }
            set
            {
                _tempStatementsComplete = value;
                TempStatements = SplitStatements(_tempStatementsComplete);
            }
        }

        public List<string> TempStatements { get; set; }

        #endregion TempStatementsComplete

        public static List<string> SplitStatements(string sql)
        {
            List<string> result = new List<string>();
            Regex regex = new Regex(" *DELIMITER (.*)?[ \n\r]?", RegexOptions.IgnoreCase);
            //string delimiter = null;
            List<KeyValuePair<string, int>> delimiters = new List<KeyValuePair<string, int>>();
            delimiters.Add(new KeyValuePair<string, int>(";", -1));
            string[] lines = sql.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            int lineNumber = 0;
            foreach (var line in lines)
            {
                Match match = regex.Match(line);
                if (match.Success && match.Groups.Count == 2)
                {
                    delimiters.Add(new KeyValuePair<string, int>(match.Groups[1].Value, lineNumber));
                    //delimiter = match.Groups[1].Value;
                }
                lineNumber++;
            }
            delimiters.Add(new KeyValuePair<string, int>(";", lines.Length));
            KeyValuePair<string, int>? lastDelimiter = null;
            foreach (var delimPair in delimiters)
            {
                if (lastDelimiter == null)
                {
                    lastDelimiter = delimPair;
                    continue;
                }
                StringBuilder currentStatement = new StringBuilder();
                for (int i = lastDelimiter.Value.Value + 1; i < delimPair.Value; ++i)
                {
                    currentStatement.Append(lines[i]).Append(Environment.NewLine);
                }
                //Do not split if delimtier is inside a comment or inside a string
                //Regex splitRegex = new Regex(string.Format("(?<!--.*){0}", lastDelimiter.Value.Key), RegexOptions.IgnoreCase);
                //Regex splitRegex = new Regex(string.Format(@"{0}(?=(?:[^']*'[^']*')*[^']*$)", lastDelimiter.Value.Key), RegexOptions.IgnoreCase);
                Regex splitRegex =
                    new Regex(
                        string.Format(@"(?<!--.*){0}(?=(?:[^']*'[^']*')*[^']*$)",
                                      lastDelimiter.Value.Key.Replace("$", "\\$")), RegexOptions.IgnoreCase);
                foreach (var statement in splitRegex.Split(currentStatement.ToString()))
                {
                    if (!string.IsNullOrEmpty(statement.Trim()))
                        result.Add(statement.Trim());
                }
                //foreach (var statement in currentStatement.ToString().Split(new[] { lastDelimiter.Value.Key }, StringSplitOptions.RemoveEmptyEntries)) {
                //    if (!string.IsNullOrEmpty(statement.Trim()))
                //        result.Add(statement.Trim());
                //}
                lastDelimiter = delimPair;
            }
            return result;
        }

        public void SetCompleteProcedure(string proc)
        {
            CompleteProcedure = proc;
            ProcedureStatements = SplitStatements(proc);
            //Regex regex = new Regex(" *DELIMITER (.*)?[ \n\r]?");
            //string delimiter = null;
            //foreach (var line in proc.Split(new string[] { Environment.NewLine },StringSplitOptions.RemoveEmptyEntries)) {
            //    Match match = regex.Match(line);
            //    if (match.Success && match.Groups.Count == 2) {
            //        delimiter = match.Groups[1].Value;
            //        break;
            //    }
            //}
            //if (string.IsNullOrEmpty(delimiter)) {
            //    //No delimiter is certainly fishy but possible
            //    ProcedureStatements.Add(proc);
            //}
            //else {
            //    proc = Regex.Replace(proc, " *DELIMITER .*?[\r]\n", string.Empty);
            //    foreach (var statement in proc.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)) {
            //        if(!string.IsNullOrEmpty(statement.Trim()))
            //            ProcedureStatements.Add(statement.Trim());
            //    }
            //}
        }

        //Returns error message if any exists
        public string FinalizeViewInfo()
        {
            if (Languages.Count == 0) Languages.Add("de");
            //Check if there are not more descriptions than languages or no description at all
            if (Descriptions.Count > Languages.Count)
                return string.Format(
                    "Es wurden {0} Beschreibungen angegeben, aber nur {1} Sprache(n) in der -- ##LANGUAGE## Sektion angegeben.",
                    Descriptions.Count, Languages.Count);
            if (Descriptions.Count == 0)
                return
                    "Es wurden keine Beschreibung angegeben, die erste Zeile nach -- ##BEGIN_VIEW## hat das Format \"Viewname;Beschreibung_Sprache1;Beschreibung_Sprache2;...\"";
            return null;
        }

        public Dictionary<string, string> GetDescriptions()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            for (int i = 0; i < Descriptions.Count; ++i) result[Languages[i]] = Descriptions[i];
            return result;
        }
    }
}