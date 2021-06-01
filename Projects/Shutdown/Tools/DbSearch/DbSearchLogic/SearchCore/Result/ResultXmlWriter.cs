using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DbSearchLogic.SearchCore.Structures.Result;
using DbSearchLogic.Structures.TableRelated;

namespace DbSearchLogic.SearchCore.Result {
    internal static class ResultXmlWriter {
        //public static void CreateResultXml(XmlWriter writer, ResultSet resultSet) {
        //    writer.WriteStartDocument();
        //    writer.WriteStartElement("Results");
        //    writer.WriteAttributeString("Count", resultSet.ColumnResults.Count.ToString());
        //    Dictionary<RowEntry, int> rowEntryToId = resultSet.Query.RowEntryToId();
        //    foreach (var columnResult in resultSet.ColumnResults) {
        //        writer.WriteStartElement("ColumnResult");
        //        writer.WriteAttributeString("ColIndex", resultSet.Query.IndexOfColumn(columnResult.Column).ToString());
        //        WriteTableHits(writer, columnResult, rowEntryToId);
        //        writer.WriteEndElement();
        //    }

        //    writer.WriteEndElement();
        //    writer.WriteEndDocument();
        //}

        //public static void WriteTableHits(XmlWriter writer, ColumnResult columnResult, Dictionary<RowEntry, int> rowEntryToId) {
        //    writer.WriteStartElement("TableHits");
        //    foreach (var tableHit in columnResult.InTable) {
        //        writer.WriteStartElement("TH");
        //        writer.WriteAttributeString("N", tableHit.TableInfo.Name);
        //        writer.WriteAttributeString("C", tableHit.TableInfo.Count.ToString());
        //        WriteColumnHitInfos(writer, tableHit, rowEntryToId);
        //        writer.WriteEndElement();
        //    }
        //    writer.WriteEndElement();
        //}

        //public static void WriteColumnHitInfos(XmlWriter writer, TableHit tableHit, Dictionary<RowEntry, int> rowEntryToId) {
        //    writer.WriteStartElement("CH");
        //    foreach (var hitInfo in tableHit.HitInfos) {
        //        writer.WriteStartElement("HI");
        //        writer.WriteAttributeString("CN", hitInfo.ColumnName);
        //        writer.WriteAttributeString("CT", ((int)hitInfo.ColumnType).ToString());
        //        foreach (var searchValueResult in hitInfo.Results) {
        //            writer.WriteStartElement("SR");
        //            writer.WriteAttributeString("I", rowEntryToId[searchValueResult.SearchValue.RowEntry].ToString());
        //            foreach (var hit in searchValueResult.Hits) {
        //                writer.WriteStartElement("H");
        //                writer.WriteAttributeString("I", hit.Id.ToString());
        //                writer.WriteAttributeString("T", ((int)hit.Type).ToString());
        //                writer.WriteEndElement();
        //            }
        //            writer.WriteEndElement();
        //        }
        //        writer.WriteEndElement();
        //    }
        //    writer.WriteEndElement();
        //}
    }
}
