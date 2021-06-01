using System;
using System.IO;
using System.Text;
using DbAccess;
using DbAccess.DbSpecific.MySQL;
using ViewboxDb.RowNoFilters;
using System.Linq;
using SystemDb;

namespace ViewAssistantBusiness.Models
{
    public enum GenerationStatus
    {
        Successful,
        Error
    }

    public class ScriptGenerationModel
    {
        private const string PARAM = "_param";

        #region Properties

        public String Path { get; set; }
        public TableModel Table { get; set; }
        public ILanguageCollection Languages { get; set; }

        #endregion Properties

        #region ScriptFileGenerationFinished

        public delegate void ScriptFileGenerationFinishedDelegate(object sender, GenerationStatus e);
        public event ScriptFileGenerationFinishedDelegate ScriptFileGenerationFinished;

        protected void OnScriptFileGenerationFinished(GenerationStatus e)
        {
            if (ScriptFileGenerationFinished != null) ScriptFileGenerationFinished(this, e);
        }
        #endregion ScriptFileGenerationFinished

        #region GenerateScriptFile

        public void GenerateScriptFile(IDatabase conn)
        {
            try
            {
                using (var file = new StreamWriter(Path, false, Encoding.UTF8))
                {
                    file.WriteLine("-- ##BEGIN_VIEW##");
                    file.WriteLine("");

                    file.Write(Table.Name + "_view");

                    foreach (var lang in Languages)
                    {
                        string value = Table.Info.Descriptions[lang];

                        file.Write(";" + value);
                    }

                    file.WriteLine("");
                    file.WriteLine("-- ##OPTIMIZE##");
                    file.WriteLine("");

                    if (Table.MandtCol != null)
                    {
                        file.WriteLine("MANDT;" + Table.MandtCol.Name);
                    }
                    if (Table.BukrsCol != null)
                    {
                        file.WriteLine("BUKRS;" + Table.BukrsCol.Name);
                    }
                    if (Table.GjahrCol != null)
                    {
                        file.WriteLine("GJAHR;" + Table.GjahrCol.Name);
                    }

                    file.WriteLine("");
                    file.WriteLine("-- ##LANGUAGES##");
                    file.WriteLine("");

                    foreach (var lang in Languages)
                    {
                        if (lang.CountryCode == Languages.Last().CountryCode)
                            file.WriteLine(lang.CountryCode);
                        else
                            file.Write(lang.CountryCode + ";");
                    }

                    file.WriteLine("");
                    file.WriteLine("-- ##DICTIONARY##");
                    file.WriteLine("");

                    foreach (var column in Table.ViewboxColumns)
                    {
                        file.Write(column.Name);

                        foreach (var lang in Languages)
                        {
                            string value = column.Info.Descriptions[lang];

                            file.Write(";" + value);
                        }

                        file.Write("\n");
                    }

                    foreach (var column in Table.ViewboxSpecialColumns)
                    {
                        file.WriteLine(column.Name + ";" + column.FromColumn);
                    }

                    file.WriteLine("");
                    file.WriteLine("-- ##KEYS##");
                    file.WriteLine("");

                    file.WriteLine("");
                    file.WriteLine("-- ##REPORT_PARAMS##");
                    file.WriteLine("");

                    foreach (var viewboxColumn in Table.ViewboxColumns)
                    {
                        StringBuilder sb = new StringBuilder();

                        foreach (var lang in Languages)
                        {
                            string value = viewboxColumn.Info.Descriptions[lang];
                            sb.Append(";" + value);
                        }

                        WriteReportParams(file, Table.Name, viewboxColumn, sb.ToString());
                    }

                    foreach (var viewboxSpecialColumn in Table.ViewboxSpecialColumns)
                    {
                        WriteReportParams(file, Table.Name, viewboxSpecialColumn, viewboxSpecialColumn.FromColumn);
                    }

                    file.WriteLine("");
                    file.WriteLine("-- ##REPORT_FILTER##");
                    file.WriteLine("");

                    var filteredColumns =
                        Table.ViewboxColumns.Where(x => x.ViewboxParamOperator != Parser.OperatorsForAssistant.None).ToList();
                    filteredColumns.AddRange(Table.ViewboxSpecialColumns.Where(x => x.ViewboxParamOperator != Parser.OperatorsForAssistant.None));

                    for (var j = 0; j < filteredColumns.Count(); ++j)
                    {
                        WriteReportFilter(file, filteredColumns[j]);
                        if (j + 1 < filteredColumns.Count())
                        {
                            file.Write(" AND");
                        }
                        file.WriteLine("");
                    }

                    file.WriteLine("");
                    file.WriteLine("-- ##REPORT_FILTER_SPECIAL##");
                    file.WriteLine("");

                    file.Write("And(");

                    for (var j = 0; j < filteredColumns.Count(); ++j)
                    {
                        if (j != 0)
                        {
                            file.Write(",");
                        }
                        WriteReportFilterSpecial(file,
                            (filteredColumns[j].ViewboxParamOperator == Parser.OperatorsForAssistant.Between) ? filteredColumns[j].Name : "in_" + filteredColumns[j].Name,
                            filteredColumns[j].ViewboxParamOperator);
                    }

                    file.WriteLine(")");

                    file.WriteLine("");
                    file.WriteLine("-- ##VIEW##");
                    file.WriteLine("");

                    file.WriteLine("drop table if exists " + Table.Name + "_view;");
                    file.WriteLine("create table if not exists " + Table.Name + "_view");
                    file.WriteLine("SELECT");

                    if (Table.ViewboxColumns.Count == 0)
                    {
                        file.WriteLine("*");
                    }

                    var sumColumns = Table.ViewboxColumns.Select(x => x.Name).ToList();
                    sumColumns.AddRange(Table.ViewboxSpecialColumns.Select(x => x.Name));
                    var i = 0;
                    while (i < sumColumns.Count)
                    {
                        file.Write(conn.Enquote(sumColumns[i]));
                        ++i;
                        if (i != sumColumns.Count)
                        {
                            file.WriteLine(",");
                        }
                    }
                    file.WriteLine("");
                    file.WriteLine("FROM");
                    file.WriteLine(conn.Enquote(conn.DbConfig.DbName, Table.Name));
                    file.WriteLine(";");

                    file.WriteLine("");
                    file.WriteLine("-- ##END_VIEW##");

                    OnScriptFileGenerationFinished(GenerationStatus.Successful);
                }
            }
            catch
            {
                OnScriptFileGenerationFinished(GenerationStatus.Error);
            }
        }

        #endregion GenerateScriptFile

        #region privates

        private static void WriteReportFilterSpecial(StreamWriter file, string name, Parser.OperatorsForAssistant operatorsForAssistant)
        {
            if (operatorsForAssistant == Parser.OperatorsForAssistant.None)
            {
                return;
            }
            if (operatorsForAssistant == Parser.OperatorsForAssistant.Between)
            {
                WriteReportFilterSpecial(file, "von_" + name + PARAM, Parser.OperatorsForAssistant.GreaterOrEqual);
                file.Write(",");
                WriteReportFilterSpecial(file, "bis_" + name + PARAM, Parser.OperatorsForAssistant.LessOrEqual);
                return;
            }
            var fullName = name.Replace(@"\", "").Replace(@"/", "").Replace(@" ", "");

            if (!fullName.Contains(PARAM))
            {
                fullName = fullName + PARAM;
            }

            file.Write("EmptyOr(");
            file.Write(fullName);
            file.Write(",");
            file.Write(operatorsForAssistant.ToString());
            file.Write("(");
            file.Write(fullName);
            file.Write("_column,");
            file.Write(fullName);
            file.Write("))");
        }

        private static void WriteReportParams(StreamWriter file, String viewName, ColumnModel viewboxColumn, String from)
        {
            if (viewboxColumn.ViewboxParamOperator == Parser.OperatorsForAssistant.None)
            {
                return;
            }

            var sb = new StringBuilder();
            sb.Append(viewName);
            sb.Append("_view;");
            sb.Append(viewboxColumn.Name);
            sb.Append(";");

            var typeName = viewboxColumn.ViewboxInfo.DataType.ToString().ToLower();
            if (typeName == "decimal")
                typeName = "numeric";
            var fullName = viewboxColumn.Name.Replace(@"\", "").Replace(@"/", "").Replace(@" ", "");

            if (viewboxColumn.ViewboxParamOperator == Parser.OperatorsForAssistant.Between)
            {
                file.Write(sb.ToString());
                file.Write("von_");
                file.Write(fullName + PARAM);
                file.Write(";");
                file.Write(typeName);
                file.Write(";");
                file.Write(from);
                file.WriteLine(" von");

                sb.Append("bis_");
            }
            else
            {
                sb.Append("in_");
            }

            sb.Append(fullName + PARAM);
            sb.Append(";");
            sb.Append(typeName);
            sb.Append(";");
            sb.Append(from);

            if (viewboxColumn.ViewboxParamOperator == Parser.OperatorsForAssistant.Between)
            {
                sb.Append(" bis");
            }
            file.WriteLine(sb.ToString());
        }

        private static void WriteReportFilter(StreamWriter file, ColumnModel viewboxColumn)
        {
            if (viewboxColumn.ViewboxParamOperator == Parser.OperatorsForAssistant.None)
            {
                return;
            }
            if (viewboxColumn.ViewboxParamOperator == Parser.OperatorsForAssistant.Between)
            {
                WriteReportFilter(file, String.Concat("von_", viewboxColumn.Name, PARAM), viewboxColumn.Name, Parser.OperatorsForAssistant.GreaterOrEqual);
                file.WriteLine(" AND");
                WriteReportFilter(file, String.Concat("bis_", viewboxColumn.Name, PARAM), viewboxColumn.Name, Parser.OperatorsForAssistant.LessOrEqual);
            }
            else
            {
                WriteReportFilter(file, String.Concat("in_", viewboxColumn.Name, PARAM), viewboxColumn.Name,
                                              viewboxColumn.ViewboxParamOperator);
            }
        }

        private static void WriteReportFilter(StreamWriter file, string fn, string sn, Parser.OperatorsForAssistant operatorsForAssistant)
        {
            var fullName = fn.Replace(@"\", "").Replace(@"/", "").Replace(@" ", "");
            file.Write("(");
            file.Write(fullName);
            file.Write(@" = '' or ");
            file.Write(fullName);
            file.Write(@" is null or `");
            file.Write(sn);
            file.Write("` ");

            file.Write(GetOperatorValue(operatorsForAssistant));

            file.Write(" ");
            file.Write(fullName);
            file.Write(")");
        }

        private static string GetOperatorValue(Parser.OperatorsForAssistant operatorsForAssistant)
        {
            StreamWriter file;
            switch (operatorsForAssistant)
            {
                case Parser.OperatorsForAssistant.Greater:
                    return ">";
                case Parser.OperatorsForAssistant.GreaterOrEqual:
                    return ">=";
                case Parser.OperatorsForAssistant.Less:
                    return "<";
                case Parser.OperatorsForAssistant.LessOrEqual:
                    return "<=";
                case Parser.OperatorsForAssistant.Equal:
                    return "=";
                case Parser.OperatorsForAssistant.Like:
                    return "LIKE";
            }
            return "UNKNOWN";
        }

        #endregion privates
    }
}
