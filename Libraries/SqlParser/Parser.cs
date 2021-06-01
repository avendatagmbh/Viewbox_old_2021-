using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gudusoft.gsqlparser;

namespace SqlParser {
    
    public static class Parser {

        public static void Test(string sql) {

            TGSqlParser sqlParser = new TGSqlParser(TDbVendor.DbVMysql);
            sqlParser.SqlText.Text = sql;

            int success = sqlParser.Parse();
            if (success != 0) {
                System.Diagnostics.Debug.WriteLine(sqlParser.ErrorMessages);
            }

            Dictionary<string, List<string>> lColumnsForTables;

            foreach (TCustomSqlStatement statement in sqlParser.SqlStatements) {

                if (statement is TCreateTableSqlStatement) {
                    TCreateTableSqlStatement lcrt_statement = statement as TCreateTableSqlStatement;
                    TSelectSqlStatement lslt_statement = lcrt_statement.SelectStmt;
                    if (lslt_statement != null) {
                        GetColumnsAndTables lHelper = new GetColumnsAndTables(lslt_statement);
                        lColumnsForTables = lHelper.GetColumnsForTables;
                    }
                }

                if (statement is TInsertSqlStatement) {
                    TInsertSqlStatement linsrt_statment = statement as TInsertSqlStatement;
                    TSelectSqlStatement lslt_statement = linsrt_statment.subquery;
                    if (lslt_statement != null) {
                        GetColumnsAndTables lHelper = new GetColumnsAndTables(lslt_statement);
                        lColumnsForTables = lHelper.GetColumnsForTables;
                    }
                }
            }
        }
    }
}
