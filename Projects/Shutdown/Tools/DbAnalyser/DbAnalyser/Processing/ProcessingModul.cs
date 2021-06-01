using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DbAnalyser.MySqlDBCommands;
using DbAnalyser.Processing.NonSAP.Methods;
using DbAnalyser.Processing.SAP;

namespace DbAnalyser.Processing
{
    public static class ProcessingModul
    {
        public static SAPDbAnalyser dbAnalyser;
        public static string ProcessColumn(ref List<string> columnvalues, string tableName, string colName, long realRowCount)
        {
            IntAnalyser IntFilter = IntAnalyser.IntFilter;
            DecimalAnalyser DoubleFilter = DecimalAnalyser.DoubleFilter;
            DateAnalyser DateFilter = DateAnalyser.DateFilter;
            DateTimeAnalyser DateTimeFilter = DateTimeAnalyser.DateTimeFilter;
            TimeAnalyser TimeFilter = TimeAnalyser.TimeFilter;

            // checking if the column contains any characters or SPACE
            if (DbSelectCommnands.IsColumnIsString(tableName, colName, new Regex("[a-zA-Z' ']+")) == "TEXT")
            {
                return "TEXT";
            }
            // checking the whole column for INT
            if (DbSelectCommnands.IsColumnIsInt(tableName, colName, IntFilter.getRegex(), realRowCount) == "INT")
            {
                return "INT";
            }
            // checking the whole column for DECIMAL
            if (DbSelectCommnands.IsColumnIsDecimal(tableName, colName, DoubleFilter.getRegex(), realRowCount) == "DECIMAL")
            {
                return "DECIMAL";
            }

            string foundedType = "unknown";

            foreach (var item in columnvalues)
            {
                switch (foundedType)
                {
                    case "unknown":
                        foundedType = foundedType == "unknown" ? DateTimeFilter.AnalyzeInput(item) : foundedType;
                        foundedType = foundedType == "unknown" ? DateFilter.AnalyzeInput(item) : foundedType;
                        foundedType = foundedType == "unknown" ? TimeFilter.AnalyzeInput(item) : foundedType;
                        foundedType = foundedType == "unknown" ? "TEXT" : foundedType;
                        break;
                    case "DATETIME":
                        foundedType = DateTimeFilter.AnalyzeInput(item) != "DATETIME" ? "unknown" : foundedType;
                        break;
                    case "DATE":
                        foundedType = DateFilter.AnalyzeInput(item) != "DATE" ? "unknown" : foundedType;
                        break;
                    case "TIME":
                        foundedType = TimeFilter.AnalyzeInput(item) != "TIME" ? "unknown" : foundedType;
                        break;
                }
                if (foundedType == "TEXT") break;
            }

            if (foundedType == "unknown")
            {
                foundedType = "TEXT";
            }

            return foundedType;
        }

        public static string ProcessColumnFast(ref List<string> columnvalues, string tableName, string colName, long realRowCount)
        {
            IntAnalyser IntFilter = IntAnalyser.IntFilter;
            DecimalAnalyser DoubleFilter = DecimalAnalyser.DoubleFilter;
            DateAnalyser DateFilter = DateAnalyser.DateFilter;
            DateTimeAnalyser DateTimeFilter = DateTimeAnalyser.DateTimeFilter;
            TimeAnalyser TimeFilter = TimeAnalyser.TimeFilter;

            string foundedType = "unknown";

            foreach (var item in columnvalues)
            {
                switch (foundedType)
                {
                    case "unknown":
                        foundedType = foundedType == "unknown" ? IntFilter.AnalyzeInput(item) : foundedType;
                        foundedType = foundedType == "unknown" ? DoubleFilter.AnalyzeInput(item) : foundedType;
                        foundedType = foundedType == "unknown" ? DateTimeFilter.AnalyzeInput(item) : foundedType;
                        foundedType = foundedType == "unknown" ? DateFilter.AnalyzeInput(item) : foundedType;
                        foundedType = foundedType == "unknown" ? TimeFilter.AnalyzeInput(item) : foundedType;
                        foundedType = foundedType == "unknown" ? "TEXT" : foundedType;
                        break;
                    case "INT":
                        foundedType = IntFilter.AnalyzeInput(item) != "INT" ? "unknown" : foundedType;
                        break;
                    case "DECIMAL":
                        foundedType = DoubleFilter.AnalyzeInput(item) != "DECIMAL" ? "unknown" : foundedType;
                        break;
                    case "DATETIME":
                        foundedType = DateTimeFilter.AnalyzeInput(item) != "DATETIME" ? "unknown" : foundedType;
                        break;
                    case "DATE":
                        foundedType = DateFilter.AnalyzeInput(item) != "DATE" ? "unknown" : foundedType;
                        break;
                    case "TIME":
                        foundedType = TimeFilter.AnalyzeInput(item) != "TIME" ? "unknown" : foundedType;
                        break;
                }
                if (foundedType == "TEXT") break;
            }

            if (foundedType == "unknown")
            {
                foundedType = "TEXT";
            }

            return foundedType;
        }
        /**
         * Gets the longest value's lenght of the selected column
         */
        public static int GetLongestTextInCol(string tableName, string colName)
        {
            return DbSelectCommnands.ReadRequiredColumnLength(tableName, colName);
        }

        public static string GetLongestTextInColumn(string tableName, string colName)
        {
            return DbSelectCommnands.ReadLongestColumnLength(tableName, colName);
        }

        public static string GetDecimalCaster(string tableName, string colName, string longestText)
        {
            string input = longestText;
            if (input.Contains('.') && input.Contains(','))
            {
                return String.Format("if(right(replace(replace(`{0}`, '.', ''), ',', '.'),1) = '-', concat('-', replace(replace(replace(`{0}`, '.', ''), ',', '.'), '-', '')), replace(replace(`{0}`, '.', ''), ',', '.')),", colName);
            }
            return String.Format("if(right(replace(`{0}`, ',', '.'),1) = '-', concat('-', replace(replace(`{0}`, ',', '.'), '-', '')), replace(`{0}`, ',', '.')),", colName);
        }

        public static bool IsColUnique(string tableName, string colName)
        {
            // TODO
            return false;
        }

        public static bool EmptyColumn(ref List<string> columnvalues)
        {
            if (columnvalues.Count == 0)
            {
                return true;
            }
            return false;
        }

        public static List<SAPReturnValue> ProcessSAPTable(List<string> columName, string actualTableName)
        {
            return dbAnalyser.AnalyseTableSAP(columName, actualTableName);
        }
    }
}
