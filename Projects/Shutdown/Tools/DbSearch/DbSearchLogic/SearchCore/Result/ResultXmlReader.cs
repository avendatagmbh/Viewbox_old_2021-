using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using DbAccess.Structures;
using DbSearchLogic.SearchCore.SearchMatrix;
using DbSearchLogic.SearchCore.Structures.Db;
using DbSearchLogic.SearchCore.Structures.Result;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearchLogic.SearchCore.Result {
    internal static class ResultXmlReader {
    //    public static void ReadResultSet(ResultSet resultSet, XmlReader reader) {
    //        Dictionary<int, RowEntry> idToRowEntry = resultSet.Query.IdToRowEntry();
    //        while (reader.Read()) {
    //            if (reader.Name == "ColumnResult" && reader.NodeType == XmlNodeType.Element) {
    //                int columnIndex = Convert.ToInt32(reader.GetAttribute("ColIndex"));
    //                ColumnResult columnResult = new ColumnResult(resultSet.Query.Columns[columnIndex]);
    //                ReadTableHits(reader, columnResult, resultSet.Query, idToRowEntry);
    //                resultSet.ColumnResults.Add(columnResult);
    //            }
    //        }
    //    }

    //    private static void ReadTableHits(XmlReader reader, ColumnResult columnResult, Query query, Dictionary<int, RowEntry> idToRowEntry) {
    //        while (reader.Read() && !(reader.Name == "TableHits" && reader.NodeType == XmlNodeType.EndElement)) {
    //            if (reader.Name == "TH") {
    //                TableHit tableHit = new TableHit(
    //                    new TableInfo(reader.GetAttribute("N"), Convert.ToInt32(reader.GetAttribute("C")), 0)
    //                    );
    //                ReadColumnHitInfos(reader, columnResult, tableHit, query, idToRowEntry);
    //                columnResult.InTable.Add(tableHit);
    //            }
    //        }
    //    }

    //    private static void ReadColumnHitInfos(XmlReader reader, ColumnResult columnResult, TableHit tableHit,
    //                                           Query query, Dictionary<int, RowEntry> idToRowEntry) {
    //        while (reader.Read() && !(reader.Name == "CH" && reader.NodeType == XmlNodeType.EndElement)) {
    //            if (reader.Name == "HI") {
    //                string columnName = reader.GetAttribute("CN");
    //                DbColumnTypes columnType =
    //                    (DbColumnTypes) Enum.Parse(typeof (DbColumnTypes), reader.GetAttribute("CT"));
    //                List<SearchValueResult> searchValueResults = ReadSearchValueResults(reader, columnResult, query, idToRowEntry);
    //                ColumnHitInfo hitInfo = new ColumnHitInfo(columnName, columnType, searchValueResults);
    //                tableHit.HitInfos.Add(hitInfo);
    //            }

    //        }
    //    }

    //    private static List<SearchValueResult> ReadSearchValueResults(XmlReader reader, ColumnResult columnResult,
    //                                                                  Query query, Dictionary<int, RowEntry> idToRowEntry) {
    //        List<SearchValueResult> searchValueResults = new List<SearchValueResult>();
    //        while (reader.Read() && reader.Name == "SR") {
    //            if (reader.NodeType == XmlNodeType.Element) {
    //                //TODO: Need to assign an Id when saving the snapshot of the query
    //                //And when reading creating a map between id and rowEntry
    //                int rowEntryId = Convert.ToInt32(reader.GetAttribute("I"));
    //                RowEntry entry = idToRowEntry[rowEntryId];
    //                SearchValueResult result = new SearchValueResult(
    //                    new SearchValue(entry.DisplayString, CultureInfo.CurrentCulture, columnResult.Column, entry));
    //                List<SearchValueResultEntry> searchValueResultEntries = ReadSearchValueResultEntries(reader);
    //                result.AddHits(searchValueResultEntries);
    //            }
    //        }
    //        return searchValueResults;
    //    }

    //    private static List<SearchValueResultEntry> ReadSearchValueResultEntries(XmlReader reader) {
    //        List<SearchValueResultEntry> result = new List<SearchValueResultEntry>();
    //        while (reader.Read() && reader.Name == "H") {
    //            if (reader.NodeType == XmlNodeType.Element) {
    //                result.Add(new SearchValueResultEntry(
    //                               Convert.ToUInt32(reader.GetAttribute("I")),
    //                               (SearchValueResultEntryType)
    //                               Enum.Parse(typeof (SearchValueResultEntryType), reader.GetAttribute("T"))
    //                               ));
    //            }
    //        }
    //        return result;
    //    }
    }
}
