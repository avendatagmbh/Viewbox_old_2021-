// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-06
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Business.Enums;
using Business.Structures;
using Config.DbStructure;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using Utils;
using Business.Interfaces;
using Business.Structures.DateReaders;
using DbAccess;
using GenericOdbc;

namespace Business {
    internal class Exporter : IExporter {
        internal Exporter(IProfile profile) {
            Profile = profile;
            TransferProgress = new TransferProgress();
            UICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
            OutputConfig = (Config.Config.GdpduOutputConfig)profile.OutputConfig.Config;
        }

        
        #region events
        public event EventHandler Finished;
        public void OnFinished() { if (Finished != null) Finished(this, new EventArgs()); }
        #endregion

        private CultureInfo UICulture { get; set; }
        private bool IsCancelled { get; set; }
        private IProfile Profile { get; set; }
        private Config.Config.GdpduOutputConfig OutputConfig { get; set; }
        private List<ITable> Tables { get; set; }
        private List<IDataReader> TableDataReader { get; set; }

        #region TransferProgress
        private TransferProgress _transferProgress;
        
        public ITransferProgress TransferProgress {
            get { return _transferProgress; }
            set { _transferProgress = (TransferProgress)value; }
        }
        #endregion TransferProgress

        public void Start() { Task.Factory.StartNew(Export); }

        public void Cancel() {
            IsCancelled = true;
            lock (TableDataReader) {
                foreach (IDataReader tableDataReader in TableDataReader)
                    tableDataReader.Cancel();
            }
        }

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

        private void AddTableDataReader(IDataReader reader) {
            lock (TableDataReader) {
                TableDataReader.Add(reader);
            }
        }

        private void RemoveTableDataReader(IDataReader reader) {
            lock (TableDataReader) {
                TableDataReader.Remove(reader);
            }
        }

        /// <summary>
        /// Exports all selected tables.
        /// </summary>
        private void Export() {

            System.Threading.Thread.CurrentThread.CurrentUICulture = UICulture;

            try {

                if (!Directory.Exists(OutputConfig.Folder))
                    Directory.CreateDirectory(OutputConfig.Folder);

                // init table list
                Tables = new List<ITable>();
                foreach (ITable table in Profile.Tables.Where(table => table.DoExport)) {
                    if (IsCancelled) break;
                    Tables.Add(table);
                }

                TransferProgress.EntitiesTotal = Tables.Count;
                TransferProgress.Step = TransferProgressSteps.ExportingTables;

                if (IsCancelled) return;

                TableDataReader = new List<IDataReader>();

                // init export tasks
                int taskCount = Math.Min(Environment.ProcessorCount, 4);
                var tasks = new Task[taskCount];
                for (int i = 0; i < taskCount; i++) {
                    tasks[i] = Task.Factory.StartNew(ExportTable, TaskCreationOptions.LongRunning);
                }

                // wait until all export tasks are finished or cancelled
                Task.WaitAll(tasks);

                if (!IsCancelled) {
                    System.IO.File.Copy("data\\gdpdu-01-08-2002.dtd", OutputConfig.Folder + "\\gdpdu-01-08-2002.dtd", true);
                    TransferProgress.Step = TransferProgressSteps.GeneratingIndexXml;
                    WriteIndexXml();
                }
            } catch (Exception ex) {
                TransferProgress.AddErrorMessage(ex.Message);
            } finally {
                if (!IsCancelled) OnFinished();
            }
        }

        #region WriteIndexXml
        private void WriteIndexXml() {
            var settings = new XmlWriterSettings {Indent = true, IndentChars = "  ", NewLineChars = Environment.NewLine};
            TextWriter tw = new StreamWriter(new FileStream(OutputConfig.Folder + "\\index.xml", FileMode.Create),
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
            w.WriteElementString("Name", OutputConfig.XmlName);
            w.WriteElementString("Location", OutputConfig.XmlLocation);
            w.WriteElementString("Comment", OutputConfig.XmlComment);
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
            foreach (ITable table in tables) {
                if (IsCancelled) break;
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
                if (IsCancelled) break;
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

        #region export
        /// <summary>
        /// Starts a new table exports until no more tables remain.
        /// </summary>
        private void ExportTable() {

            System.Threading.Thread.CurrentThread.CurrentUICulture = UICulture;

            ITable table;
            while (!IsCancelled && GetNextTable(out table)) {
                try {
                    ExportTable(table);
                } catch (Exception ex) {
                    TransferProgress.AddErrorMessage(ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets the next table from table list.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        private bool GetNextTable(out ITable table) {
            lock (Tables) {
                if (Tables.Count > 0) {
                    table = Tables.First();
                    Tables.RemoveAt(0);
                } else {
                    table = null;
                }
            }
            return table != null;
        }

        /// <summary>
        /// Exports the specified table.
        /// </summary>
        /// <param name="table"></param>
        private void ExportTable(ITable table) {

            var tableProgress = TransferProgress.AddProcessedEntity(table) as TransferTableProgress;
            int datasetCount = 0;

            using (var w =
                new StreamWriter(
                    new FileStream(OutputConfig.Folder + "\\" + GetCsvFileName(table.Name), FileMode.Create),
                    Encoding.UTF8))
            {
                //using (ITableDataReader r = SourceDbManager.GetTableDataReader(((IDatabaseConfig)Profile.InputConfig).ConnectionString, table))
                using (var connInput = ConnectionManager.CreateConnection("GenericODBC",
                    ((IDatabaseInputConfig)Profile.InputConfig).ConnectionString,
                    DbTemplateManager.GetTemplate(((IDatabaseInputConfig)Profile.InputConfig).DbTemplateName)))
                {
                    using (IDataReader r = new TableDataReader(connInput, table))
                    {
                        if (IsCancelled) return;

                        AddTableDataReader(r);
                        r.Load();

                        while (!IsCancelled && r.Read())
                        {
                            object[] data = r.GetData();
                            var result = new string[data.Length];
                            int pos = 0;
                            foreach (ITableColumn column in table.Columns)
                            {
                                switch (column.Type)
                                {
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
                                        else result[pos] = ((DateTime)(data[pos])).ToString("dd.MM.yyyy");
                                        break;

                                    case ColumnTypes.Time:
                                        result[pos] = "\"" + data[pos] + "\"";
                                        break;

                                    case ColumnTypes.DateTime:
                                        if (data[pos] is DBNull)
                                        {
                                            result[pos++] = "";
                                            result[pos++] = "";
                                            result[pos] = "";
                                        }
                                        else
                                        {
                                            var dt = (DateTime)(data[pos]);
                                            result[pos++] = "\"" + dt.ToString("dd.MM.yyyy hh:mm:ss") + "\"";
                                            result[pos++] = dt.ToString("dd.MM.yyyy");
                                            result[pos] = "\"" + dt.ToString("hh:mm:ss") + "\"";
                                        }

                                        break;

                                    case ColumnTypes.Binary:
                                        result[pos] = "\"" + StringUtils.ByteArrayToString((byte[])data[pos]) + "\"";
                                        break;

                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                                pos++;
                            }

                            w.WriteLine(string.Join(";", result));
                            datasetCount++;
                            if (datasetCount % 100 == 0)
                            {
                                tableProgress.DataSetsProcessed += datasetCount;
                                datasetCount = 0;
                            }
                        }

                        RemoveTableDataReader(r);
                        w.Close();
                    }
                }
            }

            TransferProgress.RemoveProcessedEntity(table);
        }
        #endregion export
    }
}