using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DbAccess;
using Taxonomy;
using Taxonomy.Enums;
using eBalanceKitBase.Structures;

namespace DatabaseManagement.DbUpgrade {
    public class InvariantCultureUpdater {

        #region [ Members ]

        private const string tableNameTaxonomyIds = "taxonomy_ids";
        private const string tableNameTaxonomyInfo = "taxonomy_info";
        private const string columnNameElemId = "elem_id";
        private const string columnNameTaxonomyId = "taxonomy_id";
        private const string columnNameCBValue = "cb_value_other";
        private const string columnNameXbrlId = "xbrl_element_id";
        private const string columnNameValue = "value";
        private const string columnNameName = "name";
        private const string columnNamePath = "path";
        private const string columnNameFileName = "filename";
        private const string columnNameType = "type";
        private const string columnNameVersion = "version";

        private readonly Dictionary<string, Taxonomy.Taxonomy> taxonomies = new Dictionary<string, Taxonomy.Taxonomy>();

        #endregion [ Members ]

        #region Constructor
        public InvariantCultureUpdater(IDatabase conn) {
            _conn = conn;
        }
        #endregion Constructor

        #region Properties
        private readonly IDatabase _conn;
        #endregion Properties

        #region Methods
        public void UpdateCommonTable(string tableName, List<string> columnNames, string keyColumn) {
            List<string> columnsToRequest = new List<string>();
            foreach (var column in columnNames)
                columnsToRequest.Add(_conn.Enquote(column));
            columnsToRequest.Add(_conn.Enquote(keyColumn));

            Dictionary<long, List<string>> valuesToUpdate = GetValuesToUpdate(columnNames, columnsToRequest, tableName);
            if (valuesToUpdate.Count == 0)
                return;

            UpdateElements(valuesToUpdate, tableName, columnNames, keyColumn);
        }

        public void UpdateTaxonomyTable(string tableName, List<string> columnNames, string keyColumn ) {

            int batchSize = 1000;
            long fromId = 0;
            long toId = 0;
            Dictionary<long, List<string>> elementsUpdated;
            List<Tuple<long, long>> idBatches = new List<Tuple<long, long>>();

            string sql = "SELECT " + _conn.Enquote(keyColumn) + " FROM " + _conn.Enquote(tableName);
            using (IDataReader reader = _conn.ExecuteReader(sql)) {
                int i = 0;
                while (reader.Read()) {
                    if (i == 0) fromId = Convert.ToInt64(reader[0]);
                    toId = Convert.ToInt64(reader[0]);
                    i++;
                    if (i % batchSize == 0) {
                        i = 0;
                        idBatches.Add(new Tuple<long, long>(fromId, toId));
                        toId = 0;
                    }
                }
                if (toId != 0) idBatches.Add(new Tuple<long, long>(fromId, toId));
            }

            foreach (Tuple<long, long> idRange in idBatches) {
                elementsUpdated = GetElementsToUpdate(columnNames, keyColumn, tableName, idRange.Item1, idRange.Item2);
                if (elementsUpdated.Count == 0)
                    continue;

                UpdateElements(elementsUpdated, tableName, columnNames, keyColumn);
            }
        }

        public void UpdateElements(Dictionary<long, List<string>> elementsUpdated, string tableName, List<string> columnNames, string keyColumn) {
            List<string> updateSqls = new List<string>();
            try {
                _conn.SetHighTimeout();
                _conn.BeginTransaction();

                foreach (KeyValuePair<long, List<string>> element in elementsUpdated) {
                    StringBuilder columnUpdateString = new StringBuilder();
                    for (int i = 0; i < columnNames.Count; ++i) {
                        if (!string.IsNullOrEmpty(element.Value[i]))
                            columnUpdateString.Append(_conn.Enquote(columnNames[i])).Append("=").
                                Append(_conn.GetSqlString(element.Value[i])).Append(",");
                    }
                    if (string.IsNullOrEmpty(columnUpdateString.ToString()))
                        continue;
                    columnUpdateString.Remove(columnUpdateString.Length - 1, 1);

                    updateSqls.Add(string.Format("UPDATE {0} SET {1} WHERE {2} = {3}", _conn.Enquote(_conn.DbConfig.DbName, tableName),
                                                 columnUpdateString, _conn.Enquote(keyColumn), element.Key));

                    if (updateSqls.Count > 100) {
                        ExecuteMultipleUpdates(updateSqls);
                        updateSqls.Clear();
                    }
                }

                if (updateSqls.Count > 0)
                    ExecuteMultipleUpdates(updateSqls);
                    //_conn.ExecuteNonQuery(string.Join(";", updateSqls));
                _conn.CommitTransaction();
            } catch (Exception) {
                if (_conn.HasTransaction()) _conn.RollbackTransaction();
                throw;
            }
        }

        private void ExecuteMultipleUpdates(List<string> updateSqls) {
            if (_conn.DbConfig.DbType.ToLower().Contains("oracle")) {
                foreach (var updateSql in updateSqls)
                    _conn.ExecuteNonQuery(updateSql);
            } else
                _conn.ExecuteNonQuery(string.Join(";", updateSqls));
        }

        private Dictionary<long, List<string>> GetElementsToUpdate(List<string> columnNames, string keyColumn, string tableName, long fromId, long toId) {
            List<KeyValuePair<string, Tuple<long, List<Tuple<string, string>>>>> elementsToUpdate = new List<KeyValuePair<string, Tuple<long, List<Tuple<string, string>>>>>();
            List<Tuple<string, string, string, int, string>> taxonomyList = new List<Tuple<string, string, string, int, string>>();
            Dictionary<long, List<string>> elementsUpdated = new Dictionary<long, List<string>>();

            List<string> columnsToRequest = columnNames.Select(column => _conn.Enquote(column)).ToList();

            string sql = "SELECT " + string.Join(",", columnsToRequest) + 
                            ", v." + _conn.Enquote(keyColumn) +
                            ", v." + _conn.Enquote(columnNameElemId) +
                            ", t." + _conn.Enquote(columnNameXbrlId) +
                            ", ti." + _conn.Enquote(columnNameName) +
                            ", ti." + _conn.Enquote(columnNamePath) +
                            ", ti." + _conn.Enquote(columnNameFileName) +
                            ", ti." + _conn.Enquote(columnNameType) +
                            ", ti." + _conn.Enquote(columnNameVersion) +
                            " FROM " + _conn.Enquote(tableName) + " v " +
                            "INNER JOIN " + _conn.Enquote(tableNameTaxonomyIds) + " t ON t." + _conn.Enquote(keyColumn) + " = v." + _conn.Enquote(columnNameElemId) + " " +
                            "INNER JOIN " + _conn.Enquote(tableNameTaxonomyInfo) + " ti ON t." + _conn.Enquote(columnNameTaxonomyId) + " = ti." + _conn.Enquote(keyColumn) + " " +
                            //"WHERE (" + _conn.Enquote(columnNameValue) + " IS NOT NULL AND " + "LTRIM(RTRIM(" + _conn.Enquote(columnNameValue) + ")) <> '') OR (" + _conn.Enquote(columnNameCBValue) + " IS NOT NULL AND LTRIM(RTRIM(" + _conn.Enquote(columnNameCBValue) + ")) <> '')";
                            "WHERE (v." + _conn.Enquote(keyColumn) + " >= " + fromId + " AND v." + _conn.Enquote(keyColumn) + " <= " + toId + ") AND (" + _conn.Enquote(columnNameValue) + " IS NOT NULL OR " + _conn.Enquote(columnNameCBValue) + " IS NOT NULL)";

            using (IDataReader reader = _conn.ExecuteReader(sql)) {
                while (reader.Read()) {
                    List<Tuple<string, string>> columnList = new List<Tuple<string, string>>();
                    foreach (var columnName in columnNames) {
                        string fileName = reader[columnNameFileName].ToString();
                        string value = reader[columnName].ToString().Trim();
                        if (value == string.Empty) value = null;
                        columnList.Add(new Tuple<string, string>(value, fileName));
                        if (!taxonomyList.Any(t => t.Item3 == fileName)) {
                            taxonomyList.Add(new Tuple<string, string, string, int, string>(reader[columnNameName].ToString(), reader[columnNamePath].ToString(), fileName, Convert.ToInt32(reader[columnNameType]), reader[columnNameVersion].ToString()));
                        }
                    }
                    elementsToUpdate.Add(new KeyValuePair<string, Tuple<long, List<Tuple<string, string>>>>(reader[columnNameXbrlId].ToString(), new Tuple<long, List<Tuple<string, string>>>(Convert.ToInt64(reader[keyColumn]), columnList)));
                }
            }
            
            foreach (Tuple<string, string, string, int, string> taxonomy in taxonomyList) {
                if (!taxonomies.ContainsKey(taxonomy.Item3)) {
                    TaxonomyInfo info = new TaxonomyInfo() {
                        Name = taxonomy.Item1,
                        Path = taxonomy.Item2,
                        Filename = taxonomy.Item3,
                        Type = (TaxonomyType) taxonomy.Item4,
                        Version = taxonomy.Item5
                    };
                    taxonomies.Add(taxonomy.Item3, new Taxonomy.Taxonomy(info));
                }
            }

            elementsToUpdate = elementsToUpdate.Where(
                e =>
                e.Value.Item2.Any(
                    i =>
                    taxonomies[i.Item2].Elements[e.Key].ValueType == XbrlElementValueTypes.Numeric ||
                    taxonomies[i.Item2].Elements[e.Key].ValueType == XbrlElementValueTypes.Date ||
                    taxonomies[i.Item2].Elements[e.Key].ValueType == XbrlElementValueTypes.SingleChoice ||
                    taxonomies[i.Item2].Elements[e.Key].ValueType == XbrlElementValueTypes.Monetary ||
                    taxonomies[i.Item2].Elements[e.Key].ValueType == XbrlElementValueTypes.MultipleChoice)).ToList();

            foreach (KeyValuePair<string, Tuple<long, List<Tuple<string, string>>>> element in elementsToUpdate) {
                List<string> newColumnValues = new List<string>();
                foreach (Tuple<string, string> column in element.Value.Item2) {
                    if (!string.IsNullOrEmpty(column.Item1)) {
                        // value needs to be converted
                        string newValue = null;
                        // if no conversion takes place then set the new value to null in order to avoid updating the value with itself
                        if (!ConvertValue(column.Item1, taxonomies[column.Item2].Elements[element.Key].ValueType, out newValue))
                            newValue = null;
                        newColumnValues.Add(newValue);
                    } else {
                        // value remains the same
                        newColumnValues.Add(column.Item1);
                    }
                }
                elementsUpdated.Add(element.Value.Item1, newColumnValues);
            }

            return elementsUpdated;
        }

        internal bool ConvertValue(string value, XbrlElementValueTypes types, out string newValue) {
            switch (types) {
                case XbrlElementValueTypes.Date:
                    DateTime date;
                    string exactFormat = LocalisationUtils.GermanCulture.DateTimeFormat.ShortDatePattern + " " +
                                         LocalisationUtils.GermanCulture.DateTimeFormat.LongTimePattern;
                    if (DateTime.TryParseExact(value, exactFormat, LocalisationUtils.GermanCulture, DateTimeStyles.None, out date)) {
                        newValue = date.ToString(CultureInfo.InvariantCulture);
                        return newValue != value;
                    }
                    break;
                case XbrlElementValueTypes.Monetary:
                case XbrlElementValueTypes.Numeric:
                    decimal number;
                    if (Decimal.TryParse(value, NumberStyles.Float, LocalisationUtils.GermanCulture, out number)) {
                        newValue = number.ToString(CultureInfo.InvariantCulture);
                        return newValue != value;
                    }
                    break;
                case XbrlElementValueTypes.MultipleChoice:
                    // the modification is needed, because for example the old value is :
                    // T#genInfo.report.id.reportElement.reportElements.L|T#genInfo.report.id.reportElement.reportElements.B|T#genInfo.report.id.reportElement.reportElements.BAK
                    // the new value needs 'de-gcd_' prefix for every genInfo... tag.
                    // so the string becomes:
                    // T#de-gcd_genInfo.report.id.reportElement.reportElements.L|T#de-gcd_genInfo.report.id.reportElement.reportElements.B|
                    //              T#de-gcd_genInfo.report.id.reportElement.reportElements.BAK
                    string[] splitted = value.Split(new [] {"#"}, StringSplitOptions.RemoveEmptyEntries);
                    // empty value skipped
                    if (splitted.Length > 1) {
                        StringBuilder str = new StringBuilder();
                        for (int i = 0; i < splitted.Length; ++i) {
                            str.Append(splitted[i]);
                            str.Append('#');
                            if(i < splitted.Length-1 && !splitted[i+1].StartsWith("de-gcd_"))
                                str.Append("de-gcd_");
                        }
                        // remove the last #
                        str.Remove(str.Length - 1, 1);
                        newValue = str.ToString();
                        return newValue != value;
                    }
                    break;
                case XbrlElementValueTypes.SingleChoice:
                    if (value.StartsWith("de-gcd_")) {
                        newValue = value.Substring(7);
                        return true;
                    }
                    break;
                default:
                    break;
            }
            newValue = null;
            return false;
        }

        private Dictionary<long, List<string>> GetValuesToUpdate(List<string> columnNames, List<string> columnsToRequest, string tableName) {
            string sql = "SELECT " + string.Join(",", columnsToRequest) + " FROM " + _conn.Enquote(tableName);
            Dictionary<long, List<string>> valuesToUpdate = new Dictionary<long, List<string>>();
            using (var reader = _conn.ExecuteReader(sql)) {
                while (reader.Read()) {
                    bool needValueChange = false;
                    List<string> newValues = new List<string>();
                    for (int i = 0; i < columnNames.Count; ++i) {
                        if (!reader.IsDBNull(i)) {
                            string value = reader.GetString(i);
                            string newValue;
                            if (ChangeValue(value, out newValue)) {
                                needValueChange = true;
                            }
                            newValues.Add(newValue);
                        } else
                            newValues.Add(null);
                    }
                    if (needValueChange) {
                        long id = Convert.ToInt64(reader[columnsToRequest.Count - 1]);
                        valuesToUpdate[id] = newValues;
                    }
                }
            }
            return valuesToUpdate;
        }

        internal bool ChangeValue(string value, out string newValue) {
            //Parse decimal
            decimal number;
            if (Decimal.TryParse(value, NumberStyles.Float, LocalisationUtils.GermanCulture, out number)) {
                newValue = number.ToString(CultureInfo.InvariantCulture);
                return newValue != value;
            }

            //Parse datetime
            DateTime date;
            string exactFormat = LocalisationUtils.GermanCulture.DateTimeFormat.ShortDatePattern + " " +
                                 LocalisationUtils.GermanCulture.DateTimeFormat.LongTimePattern;
            if (DateTime.TryParseExact(value, exactFormat, LocalisationUtils.GermanCulture, DateTimeStyles.None, out date)) {
                newValue = date.ToString(CultureInfo.InvariantCulture);
                return newValue != value;
            }
            newValue = value;
            return false;
        }

        #endregion Methods
    }
}
