using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DbSearchLogic.SearchCore.Result;
using DbSearchLogic.SearchCore.Structures.Db;
using log4net;
using AV.Log;

namespace DbSearchLogic.Structures.TableRelated {
    public class SearchTableDecider {

        internal ILog _log = LogHelper.GetLogger();

        #region Constructor
        public SearchTableDecider() {
        }
        #endregion Constructor

        #region Properties
        private List<string> _blackListTables = new List<string>();
        private List<string> _whiteListTables = new List<string>(); 
        #endregion Properties

        #region Methods
        public void FromString(string blackListTablesString, string whiteListTablesString){
            _blackListTables = new List<string>();
            _whiteListTables = new List<string>();

            AddTables(_blackListTables, blackListTablesString);
            AddTables(_whiteListTables, whiteListTablesString);
        }

        public void FromString(string blackListTablesString, string whiteListTablesString, Query query){
            _blackListTables = new List<string>();
            _whiteListTables = new List<string>();

            if (blackListTablesString.Contains("*") || whiteListTablesString.Contains("*")){
                string blackList = string.Empty;
                string myBlackListItem = string.Empty;
                string[] blackListItems = blackListTablesString.Split(new char[] { ';' });
                string whiteList = string.Empty;
                string myWhiteListItem = string.Empty;
                string[] whiteListItems = whiteListTablesString.Split(new char[] { ';' });
                foreach (string blackListItem in blackListItems){
                    if (blackListItem != string.Empty){
                        myBlackListItem = blackListItem.Substring(0, blackListItem.IndexOf('*'));
                        foreach (TableInfo tableInfo in query.Profile.TableInfoManager.Tables){
                            if (tableInfo.Name.StartsWith(myBlackListItem)) blackList += tableInfo.Name + ";";
                        }
                    }
                }
                foreach (string whiteListItem in whiteListItems){
                    if (whiteListItem != string.Empty){
                        myWhiteListItem = whiteListItem.Substring(0, whiteListItem.IndexOf('*'));
                        foreach (TableInfo tableInfo in query.Profile.TableInfoManager.Tables){
                            if (tableInfo.Name.StartsWith(myWhiteListItem)) whiteList += tableInfo.Name + ";";
                        }
                    }
                }
                
                AddTables(_blackListTables, blackList);
                _log.Log(LogLevelEnum.Info, "blackList: " + blackList, true);
                AddTables(_whiteListTables, whiteList);
                _log.Log(LogLevelEnum.Info, "whiteList: " + whiteList, true);
                
            }
            else{
                AddTables(_blackListTables, blackListTablesString);
                AddTables(_whiteListTables, whiteListTablesString);
            }
        }

        private void AddTables(List<string> tablesList, string tablesString) {
            if (string.IsNullOrEmpty(tablesString))
                return;
            tablesList.AddRange(from table in tablesString.Split(';') where table.Trim().Length > 0 select table.Trim());
        }

        public bool IsTableAllowed(TableInfo table) {
            if (_whiteListTables.Count > 0) {
                if (_whiteListTables.Any(whiteListTable => whiteListTable.ToLower() == table.Name.ToLower())) {
                    return true;
                }
                return false;
            }
            if (_blackListTables.Any(blackListTable => blackListTable.ToLower() == table.Name.ToLower()))
                return false;
            return true;

        }

        public string WhitelistTablesAsString() {
            return string.Join(";", _whiteListTables);
        }

        public string BlacklistTablesAsString() {
            return string.Join(";", _blackListTables);
        }

        public void ToXml(XmlWriter writer) {
            writer.WriteStartElement("STD");
                writer.WriteStartElement("BL");
                    TablesToXml(writer, _blackListTables);
                writer.WriteEndElement();
                writer.WriteStartElement("WL");
                    TablesToXml(writer, _whiteListTables);
                writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void TablesToXml(XmlWriter writer, List<string> tables ) {
            foreach (var table in tables) {
                writer.WriteStartElement("T");
                writer.WriteAttributeString("N", table);
                writer.WriteEndElement();
            }
        }

        public void FromXml(XmlReader reader) {
            if (reader.Name != "STD")
                return;

            _blackListTables.Clear();
            _whiteListTables.Clear();
            List<string> tables = _blackListTables;
            
            while (reader.Read() && !(reader.Name == "STD" && reader.NodeType == XmlNodeType.EndElement)) {
                if (reader.Name == "BL") {
                    tables = _blackListTables;
                }
                if (reader.Name == "WL") {
                    tables = _whiteListTables;
                }
                if (reader.Name == "T") {
                    tables.Add(reader["N"]);
                }
            }
        }

        #endregion Methods
    }
}
