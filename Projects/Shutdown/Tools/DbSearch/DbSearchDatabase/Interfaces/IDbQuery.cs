// --------------------------------------------------------------------------------
// author: Benjamin Held
// since: 2011-09-12
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Xml;
using DbAccess.Structures;
using DbSearchDatabase.Results;
using DbSearchDatabase.Structures;
using DbSearchDatabase.TableRelated;

namespace DbSearchDatabase.Interfaces {
    public interface IDbQuery : IDatabaseObject<int>{
        string Name { get; }
        long Count { get; }
        List<IDbColumn> Columns { get; set; }
        List<IDbRow> Rows { get; set; }
        string AdditionalInfos { get; set; }
        string TableName { get; }
        DbConfigSearchParams DbConfigSearchParams { get; }
        void LoadData(int limit, List<int> rowNumbers);
        void Save();
        bool Load();
        void AddRow(IDbRow dbRow);
        void RemoveRow(IDbRow dbRow);
        void DeleteColumns();
        void ToXml(XmlWriter xmlWriter);
        void DeleteResult(IDbResultSet result);
        void AddColumn(IDbColumn dbColumn);
        void DeleteColumn(int index);
    }
}