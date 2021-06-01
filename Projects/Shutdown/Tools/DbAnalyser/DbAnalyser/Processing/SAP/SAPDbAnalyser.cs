using System;
using System.Collections.Generic;
using System.Linq;
using DbAnalyser.MySqlDBCommands;

namespace DbAnalyser.Processing.SAP
{
    /**
     * SAPDbAnalyser analyser user Singleton pattern, because in the initialization process it's needs    
     * to load every SAP column information from the dd03l table
     */
    public class SAPDbAnalyser
    {
        private List<SAPDbData> sapData;

        public SAPDbAnalyser(List<string> tableNames)
        {
            sapData = new List<SAPDbData>();
            LoadSAPTableNames(ref tableNames);
        }        

        /**
         * loadSAPTableNames method load every SAP column information
         */
        private void LoadSAPTableNames(ref List<string> tableNames, string tableName = "dd03l")
        {
            sapData = DbSelectCommnands.ReadingAllTheSAPTableData(ref tableNames, tableName);
        }

        /**
         *  AnalyseTableSAP method processes the column informations.
         *  The dd03l table contains every columns ABAP type, which needs to be converted to SQL :)
         */
        public List<SAPReturnValue> AnalyseTableSAP(List<string> actualColumnNames, string actualTableNames)
        {
            var returnList = new List<SAPReturnValue>();
            foreach (var colName in actualColumnNames)
            {   
                var selectedSapData = sapData.FirstOrDefault(sd => sd.FieldName == colName && actualTableNames.Contains(sd.TabName.ToLower()));

                if (selectedSapData == null)
                {
                    selectedSapData = new SAPDbData(actualTableNames, colName, "TEXT", 0, 0, 0);              
                }

                switch (selectedSapData.dataType.ToUpper())
                {
                    case "RSTR":
                    case "STRG":
                        returnList.Add(new SAPReturnValue(colName, "TEXT", selectedSapData.Length, selectedSapData.Decimals));
                        break;
                    case "CHAR":
                    case "LANG":
                    case "LCHR":
                    case "CLNT":
                    case "CUKY":
                    case "UNIT":
                    case "VARC":
                    case "ACCP":
                    case "SSTR":
                    case "":
                        returnList.Add(new SAPReturnValue(colName, "TEXT", selectedSapData.Length, selectedSapData.Decimals));
                        break;
                    case "LRAW":
                        returnList.Add(new SAPReturnValue(colName, "TEXT", selectedSapData.Length, selectedSapData.Decimals));
                        break;
                    case "FLTP":
                        returnList.Add(new SAPReturnValue(colName, "TEXT", selectedSapData.Length, selectedSapData.Decimals));
                        break;
                    case "NUMC":
                    case "INT4":
                    case "INT2":
                    case "INT1":
                        if (selectedSapData.Decimals == 0)
                        {
                            returnList.Add(new SAPReturnValue(colName, "INT", selectedSapData.Length, selectedSapData.Decimals));
                        }
                        else
                        {
                            // decimals in int ?
                        }
                        break;
                    case "CURR":
                    case "DEC":
                    case "QUAN":
                        returnList.Add(new SAPReturnValue(colName, "DECIMAL", selectedSapData.Length, selectedSapData.Decimals));
                        break;
                    case "DATS":
                        returnList.Add(new SAPReturnValue(colName, "DATE", selectedSapData.Length, selectedSapData.Decimals));
                        break;
                    case "TIMS":
                        returnList.Add(new SAPReturnValue(colName, "TIME", selectedSapData.Length, selectedSapData.Decimals));
                        break;
                    default:
                        returnList.Add(new SAPReturnValue(colName, "TEXT", selectedSapData.Length, selectedSapData.Decimals));
                        break;
                }
            }
            return returnList;
        }
    }
}
