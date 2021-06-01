using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gudusoft.gsqlparser;
using System.Text.RegularExpressions;

namespace SqlParser
{
    public class UsedTablesCollector
    {
        /// <summary>
        /// Some table name begins with number. This const will be used for temporary table name creation.
        /// </summary>
        private const string TABLEPREFIX = "$@$";

        /// <summary>
        /// Returns all the used table names in a script.
        /// </summary>
        /// <param name="p_ScriptText">The script to parse.</param>
        /// <returns>List of strings containing the used table names.</returns>
        public List<string> GetUsedTablesListFromScript(string p_ScriptText)
        {
            List<string> m_TableList = new List<string>();

            TGSqlParser sqlParser = new TGSqlParser(TDbVendor.DbVMysql);

            //The input script must be temporarly changed to extend table names starting with numbers.
            sqlParser.SqlText.Text = GetScriptWithTempTableNames(p_ScriptText);
            
            int success = sqlParser.Parse();
            if (success != 0)
            {
                System.Diagnostics.Debug.WriteLine(sqlParser.ErrorMessages);
            }

            //Call recoursive process on each statements.
            foreach (TCustomSqlStatement statement in sqlParser.SqlStatements)
            {
                m_TableList.AddRange(GetTableListRecoursive(statement));
            }

            return m_TableList;
        }

        /// <summary>
        /// Extends table names starting with numbers with the const string.
        /// </summary>
        /// <param name="sqlScript">The script to extend.</param>
        /// <returns>The extended script.</returns>
        private string GetScriptWithTempTableNames(string sqlScript)
        {
            StringBuilder sb = new StringBuilder(sqlScript);
            Match match = null;

            while ((match = Regex.Match(sb.ToString(), @"\s\d{1,2}[\w\S]*[A-z]+[\w\S]*", RegexOptions.None)) != null
                && match.Success)
            {
                string tableName = match.Value.Trim();
                string tempTableName = TABLEPREFIX + tableName;

                sb = sb.Replace(tableName, tempTableName, match.Index, tempTableName.Length);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Processes a statement and retrieves all the used tables from it.
        /// </summary>
        /// <param name="p_Statement">The statement to process.</param>
        /// <returns>List of strings containing the used table names.</returns>
        private List<string> GetTableListRecoursive(TCustomSqlStatement p_Statement)
        {
            List<string> m_TableList = new List<string>();

            if (p_Statement != null)
            {
                //For each childnodes call this method.
                if (p_Statement.ChildNodes != null && p_Statement.ChildNodes.Count() > 0)
                {
                    for (int i = 0; i < p_Statement.ChildNodes.Count(); i++)
                    {
                        m_TableList.AddRange(GetTableListRecoursive(p_Statement.ChildNodes[i] as TCustomSqlStatement));
                    }
                }

                //Get the tokens from the statement and retrieve the table names from it.
                TSourceTokenList m_TokenList = p_Statement.TableTokens;
                TSourceToken m_Token = null;

                for (int i = 0; i < m_TokenList.Count(); i++)
                {
                    m_Token = m_TokenList[i];

                    //Remove the apostrofe and the special prefix from the final table name.
                    if (!m_TableList.Contains(m_Token.AsText))
                        m_TableList.Add(m_Token.AsText.Replace("`", "").Replace(TABLEPREFIX, string.Empty));
                }

                foreach (TLzTable t in p_Statement.Tables)
                {
                    if (t.SubQuery != null)
                        m_TableList.AddRange(GetUsedTablesListFromScript(t.SubQuery.PrettySql));
                    //Remove the apostrofe and the special prefix from the final table name.
                    else if (!m_TableList.Contains(t.TableFullname))
                        m_TableList.Add(t.TableFullname.Replace("`", "").Replace(TABLEPREFIX, string.Empty));
                }

                return m_TableList;
            }

            return m_TableList;
        }
    }
}
