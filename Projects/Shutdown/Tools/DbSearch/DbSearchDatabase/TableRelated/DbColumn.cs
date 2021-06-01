// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-12
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using DbAccess;
using DbAccess.Structures;
using DbSearchDatabase.Interfaces;
using DbSearchDatabase.Structures;

namespace DbSearchDatabase.TableRelated {
    [DbTable("query_columns", ForceInnoDb = true)]
    internal class DbColumn : DatabaseObjectBase<int>, IDbColumn {
        #region Constructor
        private void Init() {
            DbConfigSearchParams = new DbConfigSearchParams();
            IsVisible = true;
            IsUsedInSearch = true;
        }
        public DbColumn() {
            DbColumnInfo = new DbColumnInfo();
            Init();
        }

        public DbColumn(DbColumnInfo columnInfo, DbQuery dbQuery) {
            DbColumnInfo = columnInfo;
            DbQuery = dbQuery;
            Init();
            //Do inString Search only for columns of non text type
            if (columnInfo.Type != DbColumnTypes.DbText && columnInfo.Type != DbColumnTypes.DbLongText)
                DbConfigSearchParams.InStringSearch = false;
        }
        #endregion

        #region Properties
        [DbColumn("query_id", IsInverseMapping = true)]
        internal DbQuery DbQuery { get; set; }

        #region Name
        [DbColumn("name", Length = 512)] //public string Name { get { return DbColumnInfo.Name; } set { DbColumnInfo.Name = value; } }
        public string Name {
            get {
                if(string.IsNullOrEmpty(_name)) return OriginalColumnName; 
                return _name;
            }
            set { _name = value; }
        }
        private string _name;
        #endregion Name

        //Stores the name of the column as it was in the database
        [DbColumn("orig_name", Length = 512)] 
        public string OriginalColumnName { get { return DbColumnInfo.Name; } set { DbColumnInfo.Name = value; } }

        [DbColumn("type")]
        public DbColumnTypes Type { get { return DbColumnInfo.Type; } set { DbColumnInfo.Type = value; } }

        [DbColumn("is_userdefined")]
        public bool IsUserDefined { get; set; }

        [DbColumn("is_visible")]
        public bool IsVisible { get; set; }

        [DbColumn("used_in_search")]
        public bool IsUsedInSearch { get; set; }

        [DbColumn("comment", Length = 512)]
        public string Comment { get; set; }

        [DbColumn("display_index")]
        public int DisplayIndex { get; set; }

        [DbColumn("rules", Length = 65536)]
        public string Rules { get; set; }

        [DbColumn("entries", Length = 1000000)]
        internal string Entries { get; set; }

        
        [DbColumn("search_params", Length = 512)]
        public string DbConfigSearchParamsString {
            get { return _dbConfigSearchParamsString; }
            set {
                _dbConfigSearchParamsString = value;
                using (XmlReader reader = XmlReader.Create(new StringReader(_dbConfigSearchParamsString))) {
                    while(reader.Read())
                        DbConfigSearchParams.FromXml(reader);
                }
            }
        }
        private string _dbConfigSearchParamsString;

        public DbConfigSearchParams DbConfigSearchParams { get; private set; }

        public DbColumnInfo DbColumnInfo { get; internal set; }
        public bool IsSelected { get; set; }


        #endregion Properties

        #region Methods

        #region Save
        private void WriteEntries(XmlWriter xmlWriter, int colIndex) {
            xmlWriter.WriteStartElement("Entries");
            foreach (var row in DbQuery.Rows) {
                row.RowEntries[colIndex].ToXml(xmlWriter);
            }
            xmlWriter.WriteEndElement();
        }

        public void Save(IDatabase conn, int colIndex) {
            StringWriter sw = new StringWriter();
            using (XmlWriter xmlWriter = XmlWriter.Create(sw)) {
                xmlWriter.WriteStartDocument();
                WriteEntries(xmlWriter, colIndex);
                xmlWriter.WriteEndDocument();

            }
            Entries = sw.ToString();

            //Write Search parameter as xml string
            StringWriter stringWriter = new StringWriter();
            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter)) {DbConfigSearchParams.ToXml(xmlWriter);}
            DbConfigSearchParamsString = stringWriter.ToString();

            conn.DbMapping.Save(this);
        }
        #endregion Save

        public void ToXml(XmlWriter xmlWriter, int colIndex) {
            xmlWriter.WriteStartElement("Column");
            xmlWriter.WriteAttributeString("Name", Name);
            xmlWriter.WriteAttributeString("Type", ((int)Type).ToString());
            xmlWriter.WriteAttributeString("IsUserDefined", IsUserDefined.ToString());
            xmlWriter.WriteAttributeString("IsVisible", IsVisible.ToString());
            xmlWriter.WriteAttributeString("DisplayIndex", DisplayIndex.ToString());
            xmlWriter.WriteAttributeString("IsUsedInSearch", IsUsedInSearch.ToString());
            xmlWriter.WriteAttributeString("Comment", Comment);
            xmlWriter.WriteAttributeString("Rules", Rules);
            
            WriteEntries(xmlWriter, colIndex);
            DbConfigSearchParams.ToXml(xmlWriter);

            xmlWriter.WriteEndElement();
        }

        public static DbColumn FromXml(XmlReader reader, List<IDbRow> rows, int colIndex, DbQuery query, int columnCount) {
            DbColumn column = new DbColumn() {
                                                 DbQuery = query,
                                                 Name = reader.GetAttribute("Name"),
                                                 Type = (DbColumnTypes) Enum.Parse(typeof(DbColumnTypes), string.IsNullOrEmpty(reader.GetAttribute("Type")) ? "DbUnknown" : reader.GetAttribute("Type")),
                                                 IsUserDefined = Convert.ToBoolean(reader.GetAttribute("IsUserDefined")),
                                                 IsVisible = Convert.ToBoolean(reader.GetAttribute("IsVisible")),
                                                 IsUsedInSearch = Convert.ToBoolean(reader.GetAttribute("IsUsedInSearch")),
                                                 DisplayIndex = Convert.ToInt32(reader.GetAttribute("DisplayIndex")),
                                                 Comment = reader.GetAttribute("Comment"),
                                                 Rules = reader.GetAttribute("Rules")
                                             };
            
            LoadRowEntries(reader, column, rows, colIndex, query, columnCount);
            if(reader.Read()) column.DbConfigSearchParams.FromXml(reader);

            return column;
        }

        private static void LoadRowEntries(XmlReader reader, DbColumn column, List<IDbRow> rows, int colIndex, DbQuery query, int columnCount) {
            int currentRow = 0;
            while (reader.Read() && !(reader.NodeType == XmlNodeType.EndElement && reader.Name=="Entries")) {
                switch (reader.Name) {
                    case "RE":
                        if (currentRow >= rows.Count) rows.Add(new DbRow(query, 0,columnCount));
                        DbRow row = (DbRow)rows[currentRow];
                        row.RowEntries[colIndex].FromXml(reader, column, row);
                        currentRow++;
                        break;
                }
            }
            
        }

        public static void LoadRowEntries(List<DbColumn> columnsList, List<IDbRow> rows, DbQuery query) {
            //Read RowEntries from xml string
            int colIndex = 0;
            foreach (var column in columnsList) {
                using (XmlReader reader = XmlReader.Create(new StringReader(column.Entries))) {
                    LoadRowEntries(reader, column, rows, colIndex, query, columnsList.Count);
                    //int currentRow = 0;
                    //while (reader.Read()) {
                    //    switch(reader.Name) {
                    //        case "RowEntry":
                    //            if(currentRow >= rows.Count) rows.Add(new DbRow(query, 0));
                    //            DbRow row = (DbRow) rows[currentRow];
                    //            row.RowEntries[colIndex].FromXml(reader, column, row);
                    //            currentRow++;
                    //            break;
                    //    }
                    //}
                }
                colIndex++;
            }
        }
        #endregion

    }
}
