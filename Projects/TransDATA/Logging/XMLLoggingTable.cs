using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;

namespace Logging {
    public class XMLLoggingTable {

        public string Catalog { get; set; }
        public string Schema { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Filter { get; set; }
        public string TransferStateMessage { get; set; }
        public long Count { get; set; }
        public int TransferStateState { get; set; }

        // Data from log entry
        public long LogCount { get; set; }
        public long LogCountDest { get; set; }
        public DateTime LogTimestamp { get; set; }
        public TimeSpan LogDuration { get; set; }
        public string LogState { get; set; }
        public string LogError { get; set; }
        public string LogFilter { get; set; }
        public string LogUsername { get; set; }

        public List<XMLLoggingColumn> Columns { get; set; }

        public XMLLoggingTable() {
            Columns = new List<XMLLoggingColumn>();
        }

        public void WriteToXML(XmlWriter writer) {
            writer.WriteElementString("Catalog", Catalog);
            writer.WriteElementString("Schema", Schema);
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("Comment", Comment);
            writer.WriteElementString("Count", Count.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Filter", Filter);
            writer.WriteElementString("TransferStateState", TransferStateState.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("TransferStateMessage", TransferStateMessage);
            writer.WriteElementString("LogCount", LogCount.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("LogCountDest", LogCountDest.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("LogDuration", LogDuration.ToString());
            writer.WriteElementString("LogError", LogError);
            writer.WriteElementString("LogFilter", LogFilter);
            writer.WriteElementString("LogTimestamp", LogTimestamp.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("LogUsername", LogUsername);
            writer.WriteElementString("LogState", LogState);

            foreach (var xmlLoggingColumn in Columns) {
                writer.WriteStartElement("Column");
                xmlLoggingColumn.WriteToXML(writer);
                writer.WriteEndElement();
            }
        }

        public void Load(XmlNode node) {
            foreach (XmlNode childNode in node.ChildNodes) {
                switch (childNode.Name) {
                    case "Catalog":
                        Catalog = childNode.InnerText;
                        break;
                    case "Schema":
                        Schema = childNode.InnerText;
                        break;
                    case "Name":
                        Name = childNode.InnerText;
                        break;
                    case "Comment":
                        Comment = childNode.InnerText;
                        break;
                    case "Filter":
                        Filter = childNode.InnerText;
                        break;
                    case "TransferStateMessage":
                        TransferStateMessage = childNode.InnerText;
                        break;
                    case "LogState":
                        LogState = childNode.InnerText;
                        break;
                    case "LogError":
                        LogError = childNode.InnerText;
                        break;
                    case "LogFilter":
                        LogFilter = childNode.InnerText;
                        break;
                    case "LogUsername":
                        LogUsername = childNode.InnerText;
                        break;
                    case "TransferStateState":
                        TransferStateState = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
                        break;
                    case "Count":
                        Count = long.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
                        break;
                    case "LogCount":
                        LogCount = long.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
                        break;
                    case "LogCountDest":
                        LogCountDest = long.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
                        break;
                    case "LogTimestamp":
                        LogTimestamp = DateTime.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
                        break;
                    case "LogDuration":
                        LogDuration = TimeSpan.Parse(childNode.InnerText);
                        break;
                    case "Column":
                        var xmlLoggingColumn = new XMLLoggingColumn();
                        xmlLoggingColumn.Load(childNode);
                        Columns.Add(xmlLoggingColumn);
                        break;
                }
            }
        }
    }
}
