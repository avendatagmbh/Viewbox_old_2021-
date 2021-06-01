// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-11
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Business.Interfaces;
using Config.Config;
using Config.DbStructure;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Utils;

namespace Business.Structures.OutputAgents {
    internal class OutputAgentGdpdu : OutputAgentBase {
        public OutputAgentGdpdu(IProfile profile, IInputAgent inputAgent, IOutputConfig config)
            : base(profile, inputAgent, config) { }

        private GdpduOutputConfig GdpduOutputConfig { get { return (GdpduOutputConfig) Config; } }

        private static string GetCsvFileName(string tableName) {
            var invalidChars = new[] {
                (char) 34, // "
                (char) 42, // *
                (char) 47, // /
                (char) 58, // :
                (char) 60, // <
                (char) 62, // >
                (char) 63, // ?
                (char) 92, // \
                (char) 124 // | 
            };

            string tmp = invalidChars.Aggregate(tableName,
                                                (current, invalidChar) =>
                                                current.Replace(invalidChar.ToString(), "%" + ((int) invalidChar) + "%"));
            for (int i = 0; i <= 31; i++)
                tmp = tmp.Replace(((char) i).ToString(), "%" + i + "%");
            return tmp + ".csv";
        }

        public override void InitTransfer() {
            if (!Directory.Exists(GdpduOutputConfig.Folder))
                Directory.CreateDirectory(GdpduOutputConfig.Folder);
        }

        public override void ProcessEntity(ITransferEntity entity, TransferTableProgress progress, Logging.LoggingDb loggingDb, bool useAdo = false) {
            int datasetCount = 0;

            using (var w =
                new StreamWriter(
                    new FileStream(GdpduOutputConfig.Folder + "\\" + GetCsvFileName(entity.Name), FileMode.Create),
                    Encoding.UTF8)) {
                using (IDataReader r = InputAgent.GetDataReader(entity)) {
                    if (Cancelled) return;

                    OnDataReaderAdded(r);
                    r.Load();

                    while (!Cancelled && r.Read()) {
                        object[] data = r.GetData();
                        var result = new string[data.Length];
                        int pos = 0;
                        foreach (ITableColumn column in entity.Columns) {
                            switch (column.Type) {
                                case ColumnTypes.Numeric:
                                    result[pos] = data[pos].ToString();
                                    break;

                                case ColumnTypes.Bool:
                                    result[pos] = data[pos].ToString();
                                    break;

                                case ColumnTypes.Text:
                                    result[pos] = "\"" + data[pos] + "\"";
                                    break;

                                case ColumnTypes.Date:
                                    if (data[pos] is DBNull) result[pos] = "";
                                    else result[pos] = ((DateTime) (data[pos])).ToString("dd.MM.yyyy");
                                    break;

                                case ColumnTypes.Time:
                                    result[pos] = "\"" + data[pos] + "\"";
                                    break;

                                case ColumnTypes.DateTime:
                                    if (data[pos] is DBNull) {
                                        result[pos++] = "";
                                        result[pos++] = "";
                                        result[pos] = "";
                                    } else {
                                        var dt = (DateTime) (data[pos]);
                                        result[pos++] = "\"" + dt.ToString("dd.MM.yyyy hh:mm:ss") + "\"";
                                        result[pos++] = dt.ToString("dd.MM.yyyy");
                                        result[pos] = "\"" + dt.ToString("hh:mm:ss") + "\"";
                                    }

                                    break;

                                case ColumnTypes.Binary:
                                    result[pos] = "\"" + StringUtils.ByteArrayToString((byte[]) data[pos]) + "\"";
                                    break;

                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                            pos++;
                        }

                        w.WriteLine(string.Join(";", result));
                        datasetCount++;
                        if (datasetCount%100 == 0) {
                            progress.DataSetsProcessed += datasetCount;
                            datasetCount = 0;
                        }
                    }

                    OnDataReaderRemoved(r);
                    w.Close();
                }
            }


        }

        public override void CompleteTransfer() {
            System.IO.File.Copy("data\\gdpdu-01-08-2002.dtd", GdpduOutputConfig.Folder + "\\gdpdu-01-08-2002.dtd", true);
            //ExportProgress.Step = ExportProgressSteps.GeneratingIndexXml;
            WriteIndexXml();
        }

        #region WriteIndexXml
        private void WriteIndexXml() {
            var settings = new XmlWriterSettings {Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine};
            TextWriter tw = new StreamWriter(new FileStream(GdpduOutputConfig.Folder + "\\index.xml", FileMode.Create),
                                             Encoding.UTF8);
            XmlWriter w = XmlWriter.Create(tw, settings);
            w.WriteStartDocument();
            WriteDataSet(w);
            w.WriteEndDocument();
            w.Close();
        }

        private void WriteDataSet(XmlWriter w) {
            w.WriteStartElement("DataSet");
            WriteVersion(w);
            WriteDataSupplier(w);
            WriteCommand(w);
            WriteMedia(w);
            w.WriteEndElement();
        }

        private static void WriteVersion(XmlWriter w) { w.WriteElementString("Version", "3.6"); }

        private void WriteDataSupplier(XmlWriter w) {
            w.WriteStartElement("DataSupplier");
            w.WriteElementString("Name", GdpduOutputConfig.XmlName);
            w.WriteElementString("Location", GdpduOutputConfig.XmlLocation);
            w.WriteElementString("Comment", GdpduOutputConfig.XmlComment);
            w.WriteEndElement();
        }

        private void WriteCommand(XmlWriter w) {
            // not used
        }

        private void WriteMedia(XmlWriter w) {
            List<ITable> tables = Profile.Tables.Where(table => table.DoExport).ToList();
            WriteMedia(w, "CD 1", tables);
        }

        private void WriteMedia(XmlWriter w, string name, List<ITable> tables) {
            w.WriteStartElement("Media");
            w.WriteElementString("Name", name);
            foreach (ITable table in tables.TakeWhile(table => !Cancelled)) {
                WriteTable(w, table);
            }
            w.WriteEndElement();
        }

        private void WriteTable(XmlWriter w, ITable table) {
            w.WriteStartElement("Table");
            w.WriteElementString("URL", GetCsvFileName(table.Name));
            w.WriteElementString("Name", table.Name);
            w.WriteElementString("Description", table.Comment);
            w.WriteElementString("DecimalSymbol", ".");
            w.WriteElementString("DigitGroupingSymbol", ",");
            WriteColumns(w, table);
            w.WriteEndElement();
        }

        private void WriteColumns(XmlWriter w, ITable table) {
            w.WriteStartElement("VariableLength");
            foreach (ITableColumn column in table.Columns) {
                if (Cancelled) break;
                if (column.DoExport) WriteColumn(w, column);
            }
            w.WriteEndElement();
        }

        private void WriteColumn(XmlWriter w, ITableColumn tableColumn) {
            if (tableColumn.Type == ColumnTypes.DateTime) {
                WriteDateTimeColumn(w, tableColumn);
                return;
            }

            w.WriteStartElement("VariableColumn");
            w.WriteElementString("Name", tableColumn.Name);
            w.WriteElementString("Description", tableColumn.Comment);
            switch (tableColumn.Type) {
                case ColumnTypes.Numeric:
                case ColumnTypes.Bool:
                    w.WriteStartElement("Numeric");
                    w.WriteElementString("Accuracy", tableColumn.NumericScale.ToString());
                    w.WriteEndElement();
                    break;

                case ColumnTypes.Time:
                case ColumnTypes.Binary:
                case ColumnTypes.Text:
                    w.WriteElementString("AlphaNumeric", "");
                    w.WriteElementString("MaxLength", tableColumn.MaxLength.ToString());
                    break;

                case ColumnTypes.Date:
                    w.WriteStartElement("Date");
                    w.WriteElementString("Format", "DD.MM.YYYY");
                    w.WriteEndElement();
                    break;

                case ColumnTypes.DateTime:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            w.WriteEndElement();
        }

        private void WriteDateTimeColumn(XmlWriter w, ITableColumn tableColumn) {
            w.WriteStartElement("VariableColumn");
            w.WriteElementString("Name", tableColumn.Name);
            w.WriteElementString("Description", tableColumn.Comment);
            w.WriteElementString("AlphaNumeric", "");
            w.WriteElementString("MaxLength", "19");
            w.WriteEndElement();

            w.WriteStartElement("VariableColumn");
            w.WriteElementString("Name", tableColumn.Name + "__date");
            w.WriteElementString("Description", tableColumn.Comment);
            w.WriteStartElement("Date");
            w.WriteElementString("Format", "DD.MM.YYYY");
            w.WriteEndElement();
            w.WriteEndElement();

            w.WriteStartElement("VariableColumn");
            w.WriteElementString("Name", tableColumn.Name + "__time");
            w.WriteElementString("Description", tableColumn.Comment);
            w.WriteElementString("AlphaNumeric", "");
            w.WriteElementString("MaxLength", "8");
            w.WriteEndElement();
        }
        #endregion WriteIndexXml

        public override bool CheckDataAccess() {
            return true;
            // TODO
        }

        public override string GetDescription() {
            return null;
        }
    }
}