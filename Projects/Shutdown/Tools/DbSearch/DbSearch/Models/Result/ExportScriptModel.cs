using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearch.Models.Result {
    public class ExportScriptModel {
        #region Constructor
        public ExportScriptModel(Query query ) {
            StringBuilder result = new StringBuilder("SELECT " + Environment.NewLine);
            List<string> tables = new List<string>();
            foreach (var columnMapping in query.UserColumnMappings.ColumnMappings) {
                result.Append("`").Append(columnMapping.ResultTable.Name).Append("`.`").Append(
                    columnMapping.TableColumnName).Append("` AS ").Append(columnMapping.SearchColumn.Name).Append(",").Append(Environment.NewLine);
                if(!tables.Any(s => s.ToLower() == columnMapping.ResultTable.Name.ToLower()))
                    tables.Add(columnMapping.ResultTable.Name);
            }
            //Remove last ,
            result.Remove(result.Length - 3, 3);
            result.Append(Environment.NewLine).Append("FROM ").Append(string.Join(",", tables));
            result.Append(";");
            GeneratedScript = result.ToString();
        }
        #endregion Constructor

        #region Properties
        public string GeneratedScript { get; private set; }
        #endregion Properties

        #region Methods
        #endregion Methods
    }
}
