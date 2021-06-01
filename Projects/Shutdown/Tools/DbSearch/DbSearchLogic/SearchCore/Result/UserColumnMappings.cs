// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-23
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DbSearchLogic.SearchCore.Structures.Db;
using DbSearchLogic.Structures.TableRelated;
using Utils;
using log4net;
using AV.Log;

namespace DbSearchLogic.SearchCore.Result {
    public class UserColumnMappings {

        internal ILog _log = LogHelper.GetLogger();

        #region UserColumnMappings
        public UserColumnMappings(Query query) {
            ColumnMappings = new ObservableCollectionAsync<ColumnMapping>();
            _query = query;
        }
        #endregion UserColumnMappings

        #region Properties
        public ObservableCollectionAsync<ColumnMapping> ColumnMappings { get; set; }
        private Query _query;
        #endregion

        #region Methods
        public bool MappingAllowed(ColumnMapping mapping) {
            //Check if the SearchColumn is already mapped or if the result column is already mapped
            return mapping.SearchColumn.MappedTo == null &&
                   !ColumnMappings.Any(
                       cm =>
                       cm.ResultTable.Name == mapping.ResultTable.Name && cm.TableColumnName == mapping.TableColumnName);
            //return true;
            //return ColumnMappings.All(currentMappings => currentMappings.SearchColumn != mapping.SearchColumn && currentMappings.TableColumnName != mapping.TableColumnName && currentMappings.ResultTable.Name != mapping.ResultTable.Name)  && 
            //    ColumnMappings.All(currentMappings => currentMappings.SearchColumn.Name != mapping.SearchColumn.Name && currentMappings.ResultTable.Name != mapping.ResultTable.Name);
        }

        public void AddMapping(ColumnMapping mapping) {
            if (!MappingAllowed(mapping)) return;
            ColumnMappings.Add(mapping);
        }

        public void AddMapping(Column searchColumn, TableInfo viewTable, string columnName) {
            AddMapping(new ColumnMapping(searchColumn, viewTable, columnName));
        }

        public bool HasMapping(ColumnMapping columnMapping) {
            return ColumnMappings.Contains(columnMapping);
        }

        public ColumnMapping GetMapping(Column column) {
            return ColumnMappings.FirstOrDefault(currentMappings => currentMappings.SearchColumn.Name == column.Name);
        }

        public void RemoveMapping(ColumnMapping mapping) {
            ColumnMappings.Remove(mapping);
        }

        public void RemoveMapping(Column searchColumn, TableInfo tableInfo, string columnName) {
            RemoveMapping(new ColumnMapping(searchColumn, tableInfo, columnName));
        }

        public void Clear() {
            ColumnMappings.Clear();
        }

        public void ToXml(XmlWriter writer) {
            writer.WriteStartElement("CMS");
            foreach (var columnMapping in ColumnMappings) {
                writer.WriteStartElement("CM");
                //Index of search column
                writer.WriteAttributeString("SCI", columnMapping.SearchColumn.Query.IndexOfColumn(columnMapping.SearchColumn).ToString());
                //Result table name
                writer.WriteAttributeString("RT", columnMapping.ResultTable.Name);
                //Result column name
                writer.WriteAttributeString("RC", columnMapping.TableColumnName);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        public void FromXml(XmlReader reader) {
            ColumnMappings.Clear();
            if (reader.Name == "CMS" && reader.IsEmptyElement)
                return;
            while (reader.Read() && !(reader.Name == "CMS" && reader.NodeType == XmlNodeType.EndElement)) {
                if (reader.Name == "CM") {
                    try {
                        ColumnMapping mapping =
                            new ColumnMapping(_query.Columns[Convert.ToInt32(reader.GetAttribute("SCI"))],
                                              new TableInfo(reader.GetAttribute("RT"), -1),
                                              reader.GetAttribute("RC"));
                        AddMapping(mapping);
                    } catch (Exception ex) {
                        _log.Log(LogLevelEnum.Error, "Fehler beim Laden der getätigten Spaltenzuordnungen: " + ex.Message);
                    }
                }
            }
        }

        public bool HasMappings() {
            return ColumnMappings.Count != 0;
        }
        #endregion Methods

    }
}
