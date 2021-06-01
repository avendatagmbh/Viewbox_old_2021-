using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using AV.Log;
using DbAccess;
using DbAccess.Structures;
using Project.Converters;
using ProjectDb.Structures;
using ProjectDb.Tables.Base;
using Utils;
using ViewBuilderCommon;
using ViewBuilderCommon.Interfaces;
using log4net;

namespace ProjectDb.Tables
{
    /// <summary>
    ///   This class specifies a view.
    /// </summary>
    [DbTable(TABLENAME)]
    public class Viewscript : TableBase, IJobInfo
    {
        private const string TABLENAME = "viewscripts";
        private readonly int SendMailAfterDays = 3;
        private readonly ILog log = LogHelper.GetLogger();

        /// <summary>
        ///   Initializes a new instance of the <see cref="ViewState" /> class.
        /// </summary>
        public Viewscript()
        {
            ViewInfo = new ViewInfo();
            Init();
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="View" /> class.
        /// </summary>
        public Viewscript(ViewInfo viewInfo)
        {
            IsChecked = true;
            LastError = string.Empty;
            Sql = string.Empty;
            Warnings = string.Empty;
            Name = viewInfo.Name;
            ViewInfo = viewInfo;
            Init();
        }

        #region fields

        /// <summary>
        ///   Converter for state strings.
        /// </summary>
        private static readonly ViewscriptStatesToStringConverter _stateConverter =
            new ViewscriptStatesToStringConverter();

        /// <summary>
        ///   See property Error.
        /// </summary>
        private string _error;

        /// <summary>
        ///   See propery ErrorSql.
        /// </summary>
        private string _errorSql;

        /// <summary>
        ///   See propery ErrorSqlPrefix.
        /// </summary>
        private string _errorSqlPrefix;

        /// <summary>
        ///   See propery ErrorSqlSuffix.
        /// </summary>
        private string _errorSqlSuffix;

        /// <summary>
        ///   See property Fileinfo.
        /// </summary>
        private FileInfo _fileInfo;

        /// <summary>
        ///   See property Hash.
        /// </summary>
        private string _hash;

        /// <summary>
        ///   See property Indizes.
        /// </summary>
        private string _indizes;

        /// <summary>
        ///   See property IsChecked.
        /// </summary>
        private bool _isChecked;

        /// <summary>
        ///   See property IsHovered.
        /// </summary>
        private bool _isHovered;

        /// <summary>
        ///   See property ParsingError.
        /// </summary>
        private string _parsingError;

        /// <summary>
        ///   See property ParsingState.
        /// </summary>
        private bool? _parsingState;

        /// <summary>
        ///   See property Script.
        /// </summary>
        private string _script;

        /// <summary>
        ///   See property Sql.
        /// </summary>
        private string _sql;

        /// <summary>
        ///   See property state.
        /// </summary>
        private ViewscriptStates _state;

        /// <summary>
        ///   See property Tables.
        /// </summary>
        private string _tables;

        /// <summary>
        ///   See property Warnings.
        /// </summary>
        private string _warnings;

        #endregion fields

        #region persistent properties

        private bool _warningEmailSent;

        [DbColumn("user", AllowDbNull = false, Length = 64)]
        public string User { get; set; }

        /// <summary>
        ///   Gets or sets the fileName, which includes the full path, the phisical file name and extension
        /// </summary>
        /// <value> The fileName. </value>
        [DbColumn("fileName", AllowDbNull = false, Length = 2048)]
        public string FileName { get; set; }

        /// <summary>
        ///   Gets or sets the description.
        /// </summary>
        /// <value> The description. </value>
        public string Description
        {
            get { return ViewInfo.Description; }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether this instance is checked.
        /// </summary>
        /// <value> <c>true</c> if this instance is checked; otherwise, <c>false</c> . </value>
        [DbColumn("isChecked", AllowDbNull = false)]
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the last error message.
        /// </summary>
        /// <value> The last error message. </value>
        [DbColumn("error", AllowDbNull = false, Length = 100000)]
        public string LastError
        {
            get { return _error; }
            set
            {
                if (_error != value)
                {
                    _error = value;
                    OnPropertyChanged("LastError");
                    OnPropertyChanged("NoErrors");
                    ParseErrorSql();
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the script parsing is succesful or failed.
        /// </summary>
        /// <value> <c>true</c> if the script parsing is successful; otherwise, <c>false</c> . </value>
        [DbColumn("parsingState", AllowDbNull = true)]
        public bool? ParsingState
        {
            get { return _parsingState; }
            set
            {
                if (_parsingState != value)
                {
                    _parsingState = value;
                    OnPropertyChanged("ParsingState");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the parsing error.
        /// </summary>
        /// <value> parsing error </value>
        [DbColumn("parsingError", AllowDbNull = true)]
        public string ParsingError
        {
            get { return _parsingError; }
            set
            {
                if (_parsingError != value)
                {
                    _parsingError = value;
                    OnPropertyChanged("ParsingError");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the currently executed sql-statement, or the last executed sql-statement in case of errors.
        /// </summary>
        /// <value> The SQL. </value>
        [DbColumn("sql", AllowDbNull = false, Length = 100000)]
        public string Sql
        {
            get { return _sql; }
            set
            {
                if (_sql != value)
                {
                    _sql = value;
                    OnPropertyChanged("Sql");
                    ParseErrorSql();
                }
            }
        }

        [DbColumn("warnings", AllowDbNull = false, Length = 100000)]
        public string Warnings
        {
            get { return _warnings; }
            set
            {
                if (_warnings != value)
                {
                    _warnings = value;
                    OnPropertyChanged("Warnings");
                    OnPropertyChanged("NoErrors");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the tables.
        /// </summary>
        /// <value> The tables. </value>
        [DbColumn("tables", AllowDbNull = false, Length = 50000)]
        public string Tables
        {
            get { return _tables; }
            set
            {
                if (_tables != value)
                {
                    _tables = value;
                    OnPropertyChanged("Tables");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the hash.
        /// </summary>
        /// <value> The hash. </value>
        [DbColumn("hash", AllowDbNull = false, Length = 2048)]
        public string Hash
        {
            get { return _hash; }
            set
            {
                if (_hash != value)
                {
                    _hash = value;
                    OnPropertyChanged("Hash");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the indizes.
        /// </summary>
        /// <value> The indizes. </value>
        [DbColumn("indizes", AllowDbNull = true, Length = 50000)]
        public string Indizes
        {
            get { return _indizes; }
            set
            {
                if (_indizes != value)
                {
                    _indizes = value;
                    OnPropertyChanged("Indizes");
                }
            }
        }

        public bool NoErrors
        {
            get { return string.IsNullOrEmpty(LastError) && string.IsNullOrEmpty(Warnings); }
        }

        /// <summary>
        ///   Gets or sets the name.
        /// </summary>
        /// <value> The name. </value>
        [DbColumn("name", AllowDbNull = false, Length = 256)]
        public string Name { get; set; }

        /// <summary>
        ///   Gets or sets the state.
        /// </summary>
        /// <value> The state. </value>
        [DbColumn("state", AllowDbNull = false)]
        public ViewscriptStates State
        {
            get { return _state; }
            set
            {
                if (_state != value)
                {
                    _state = value;
                    OnPropertyChanged("State");
                }
            }
        }

        #endregion persistent properties

        #region non persistent properties

        /// <summary>
        ///   See property ViewInfo.
        /// </summary>
        private ViewInfo _viewInfo;

        /// <summary>
        ///   Gets or sets the script.
        /// </summary>
        /// <value> The script. </value>
        public string Script
        {
            get { return _script; }
            set
            {
                if (_script != value)
                {
                    _script = value;
                    OnPropertyChanged("Script");
                }
            }
        }

        /// <summary>
        ///   Gets the creation timestamp display string.
        /// </summary>
        /// <value> The creation timestamp display string. </value>
        public string FileTimestampDisplayString
        {
            get
            {
                if (FileInfo == null) return string.Empty;
                return FileInfo.LastWriteTime.ToString();
            }
        }

        /// <summary>
        ///   Gets or sets the fileinfo.
        /// </summary>
        /// <value> The file info. </value>
        public FileInfo FileInfo
        {
            get { return _fileInfo; }
            set
            {
                if (_fileInfo != value)
                {
                    _fileInfo = value;
                    OnPropertyChanged("FileInfo");
                    OnPropertyChanged("FileTimestampDisplayString");
                }
            }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether this instance is hovered (needed for gui).
        /// </summary>
        /// <value> <c>true</c> if this instance is hovered; otherwise, <c>false</c> . </value>
        public bool IsHovered
        {
            get { return _isHovered; }
            set
            {
                if (_isHovered != value)
                {
                    _isHovered = value;
                    OnPropertyChanged("IsHovered");
                }
            }
        }

        /// <summary>
        ///   Gets the state display string.
        /// </summary>
        /// <value> The state display string. </value>
        public string StateDisplayString
        {
            get { return (string) _stateConverter.Convert(State, typeof (string), null, CultureInfo.CurrentUICulture); }
        }

        /// <summary>
        ///   Gets the prefix of the SQL string in case of errors.
        /// </summary>
        /// <value> The error SQL. </value>
        public string ErrorSqlPrefix
        {
            get { return _errorSqlPrefix; }
            set
            {
                if (_errorSqlPrefix != value)
                {
                    _errorSqlPrefix = value;
                    OnPropertyChanged("ErrorSqlPrefix");
                }
            }
        }

        /// <summary>
        ///   Gets the SQL string in case of errors.
        /// </summary>
        /// <value> The error SQL. </value>
        public string ErrorSql
        {
            get { return _errorSql; }
            set
            {
                if (_errorSql != value)
                {
                    _errorSql = value;
                    OnPropertyChanged("ErrorSql");
                }
            }
        }

        /// <summary>
        ///   Gets the suffix of the SQL string in case of errors.
        /// </summary>
        /// <value> The error SQL. </value>
        public string ErrorSqlSuffix
        {
            get { return _errorSqlSuffix; }
            set
            {
                if (_errorSqlSuffix != value)
                {
                    _errorSqlSuffix = value;
                    OnPropertyChanged("ErrorSqlSuffix");
                }
            }
        }

        public ViewInfo ViewInfo
        {
            get { return _viewInfo; }
            set
            {
                if (_viewInfo != value)
                {
                    _viewInfo = value;
                    OnPropertyChanged("");
                    //OnPropertyChanged("Description");
                }
            }
        }

        public bool HasStoredProcedure
        {
            get { return !string.IsNullOrEmpty(ViewInfo.ProcedureName); }
        }

        public ProcedureInfo ProcedureInfo { get; set; }

        #region DurationHelper

        private readonly Stopwatch _sw = new Stopwatch();
        private DateTime _creationTimestamp;
        private TimeSpan _duration;

        public DateTime CreationTimestamp
        {
            get { return _creationTimestamp; }
            set
            {
                if (_creationTimestamp != value)
                {
                    _creationTimestamp = value;
                    OnPropertyChanged("CreationTimestamp");
                    OnPropertyChanged("CreationTimestampDisplayString");
                }
            }
        }

        /// <summary>
        ///   Gets or sets the duration to implement the view (excluding the time for index creation).
        /// </summary>
        /// <value> The duration. </value>
        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged("Duration");
                    OnPropertyChanged("DurationDisplayString");
                }
            }
        }

        public void StartStopwatch()
        {
            _sw.Reset();
            _sw.Start();
            _warningEmailSent = false;
            new Thread(UpdateDuration).Start();
        }

        /// <summary>
        ///   Stops the stopwatch and updates the creation timestamp.
        /// </summary>
        public void StopStopwatch()
        {
            CreationTimestamp = DateTime.Now;
            _sw.Stop();
            Duration = _sw.Elapsed;
        }

        /// <summary>
        ///   Gets the duration display string.
        /// </summary>
        /// <value> The duration display string. </value>
        public string DurationDisplayString
        {
            get { return StringUtils.FormatTimeSpan(Duration); }
        }

        /// <summary>
        ///   Updates the duration.
        /// </summary>
        private void UpdateDuration()
        {
            while (_sw.IsRunning)
            {
                Duration = _sw.Elapsed;
                Thread.Sleep(500);
                CheckSendEmail();
            }
        }

        #endregion DurationHelper

        #endregion non persistent properties

        #region methods

        /// <summary>
        ///   Loads the viewsscipt table.
        /// </summary>
        /// <param name="connMgr"> The conn MGR. </param>
        /// <returns> </returns>
        internal static List<Viewscript> Load(IDatabase conn, bool onlyFromUser = true)
        {
            if (onlyFromUser)
                return
                    conn.DbMapping.Load<Viewscript>(conn.Enquote("user") + "=" +
                                                    conn.GetSqlString(ActiveDirectory.GetUserAbbr()));
            return conn.DbMapping.Load<Viewscript>();
        }

        /// <summary>
        ///   Sets the IsChecked state for all views.
        /// </summary>
        public void CheckAll()
        {
            try
            {
                if (!ProjectDb.ConnectionManager.IsDisposed)
                {
                    using (IDatabase conn = ProjectDb.ConnectionManager.GetConnection())
                    {
                        conn.ExecuteNonQuery(
                            "UPDATE " + conn.Enquote(TABLENAME) +
                            " SET " + conn.Enquote("isChecked") + "= 1");
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        ///   Resets the IsChecked state for all views.
        /// </summary>
        public void UncheckAll()
        {
            try
            {
                if (!ProjectDb.ConnectionManager.IsDisposed)
                {
                    using (IDatabase conn = ProjectDb.ConnectionManager.GetConnection())
                    {
                        conn.ExecuteNonQuery(
                            "UPDATE " + conn.Enquote(TABLENAME) +
                            " SET " + conn.Enquote("isChecked") + "= 0");
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void ParseErrorSql()
        {
            if (Sql == null) return;
            string[] sqlLines = Sql.Split(new[] {Environment.NewLine}, StringSplitOptions.None);
            if (LastError.ToLower().Contains("line "))
            {
                int lineNumber;
                if (
                    !int.TryParse(
                        Regex.Match(LastError.Substring(LastError.ToLower().IndexOf("line ")), "([0-9]+)").Groups[1].
                            Value, out lineNumber))
                {
                    ErrorSqlPrefix = Sql;
                    ErrorSqlSuffix = string.Empty;
                    ErrorSql = String.Empty;
                }
                if (lineNumber - 1 > sqlLines.Length)
                {
                    ErrorSqlPrefix = Sql;
                    ErrorSqlSuffix = string.Empty;
                    ErrorSql = String.Empty;
                }
                else
                    SetErrorSqlParts(lineNumber, sqlLines);
            }
            else if (Regex.IsMatch(LastError, "Unknown column '(.+)' in 'field list'", RegexOptions.IgnoreCase))
            {
                // search line
                string columnName =
                    Regex.Match(LastError, "Unknown column '(.+)' in 'field list'", RegexOptions.IgnoreCase).Groups[1].
                        Value;
                for (int i = 0; i < sqlLines.Length; i++)
                {
                    if (sqlLines[i].Contains(columnName))
                    {
                        SetErrorSqlParts(i, sqlLines);
                        return;
                    }
                }
            }
            else
            {
                ErrorSqlPrefix = Sql;
                ErrorSqlSuffix = string.Empty;
                ErrorSql = String.Empty;
            }
        }

        /// <summary>
        ///   Sets the error SQL parts.
        /// </summary>
        /// <param name="lineNumber"> The line number. </param>
        /// <param name="sqlLines"> The SQL lines. </param>
        /// <returns> </returns>
        private void SetErrorSqlParts(int lineNumber, string[] sqlLines)
        {
            if (lineNumber >= 0 && lineNumber < sqlLines.Length)
                ErrorSql = sqlLines[lineNumber];
            ;
            int actLines = 0;
            int maxLines = 3;
            int curLineNumber = lineNumber - 1;
            ErrorSqlPrefix = string.Empty;
            while (curLineNumber > 0 && actLines < maxLines && curLineNumber < sqlLines.Length)
            {
                actLines++;
                if (ErrorSqlPrefix.Length > 0) ErrorSqlPrefix = Environment.NewLine + ErrorSqlPrefix;
                ErrorSqlPrefix = sqlLines[curLineNumber] + ErrorSqlPrefix;
                curLineNumber--;
            }
            if (curLineNumber < sqlLines.Length)
            {
                ErrorSqlPrefix = "..." + Environment.NewLine + ErrorSqlPrefix;
            }
            ErrorSqlSuffix = string.Empty;
            actLines = 0;
            curLineNumber = lineNumber + 1;
            while (curLineNumber < sqlLines.Length && actLines < maxLines)
            {
                actLines++;
                if (ErrorSqlSuffix.Length > 0) ErrorSqlSuffix += Environment.NewLine;
                ErrorSqlSuffix += sqlLines[curLineNumber];
                curLineNumber++;
            }
            if (curLineNumber < sqlLines.Length)
            {
                ErrorSqlSuffix += Environment.NewLine + "...";
            }
        }

        /// <summary>
        ///   Adds the specified warning message.
        /// </summary>
        /// <param name="warning"> The warning message. </param>
        public void AddWarning(string warning)
        {
            if (Warnings.Length > 0) Warnings += Environment.NewLine;
            Warnings += warning;
        }

        public Dictionary<string, long> GetTables()
        {
            var output = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            using (var reader = XmlReader.Create(new StringReader(Tables)))
            {
                while (reader.Read())
                {
                    if (reader.Name == "Table")
                    {
                        output.Add(reader.GetAttribute("Name"), Convert.ToInt64(reader.GetAttribute("Complexity")));
                    }
                }
            }
            return output;
        }

        public void SetTables(Dictionary<string, long> tables)
        {
            using (var memStream = new MemoryStream())
            {
                var settings = new XmlWriterSettings
                                   {Indent = true, IndentChars = ("    "), Encoding = Encoding.Default};
                using (var writer = XmlWriter.Create(memStream, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Tables");
                    foreach (var t in tables)
                    {
                        writer.WriteStartElement("Table");
                        writer.WriteAttributeString("Name", t.Key);
                        writer.WriteAttributeString("Complexity", t.Value.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    memStream.Flush();
                    memStream.Position = 0;
                    var reader = new StreamReader(memStream);
                    Tables = reader.ReadToEnd();
                }
            }
        }

        public List<Index> GetIndizes()
        {
            var output = new List<Index>();
            using (var reader = XmlReader.Create(new StringReader(Indizes)))
            {
                while (reader.Read())
                {
                    if (reader.Name == "Table")
                    {
                        var table = reader.GetAttribute("Name");
                        var sub = reader.ReadSubtree();
                        while (sub.Read())
                        {
                            if (reader.Name == "Index")
                            {
                                var index = new Index(table, reader.GetAttribute("Fields").Split(';').ToList());
                                if (!output.Contains(index))
                                {
                                    output.Add(index);
                                }
                            }
                        }
                    }
                }
            }
            return output;
        }

        public void SetIndizes(List<Index> indizes)
        {
            using (var memStream = new MemoryStream())
            {
                var settings = new XmlWriterSettings
                                   {Indent = true, IndentChars = ("    "), Encoding = Encoding.Default};
                using (var writer = XmlWriter.Create(memStream, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Indizes");
                    foreach (var index in indizes.GroupBy(i => i.Table))
                    {
                        writer.WriteStartElement("Table");
                        writer.WriteAttributeString("Name", index.Key);
                        foreach (var i in index)
                        {
                            writer.WriteStartElement("Index");
                            writer.WriteAttributeString("Fields", String.Join(";", i.Columns));
                            writer.WriteEndElement();
                        }

                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    memStream.Flush();
                    memStream.Position = 0;
                    var reader = new StreamReader(memStream);
                    Indizes = reader.ReadToEnd();
                }
            }
        }

        public string GetHash(string textToHash)
        {
            if (string.IsNullOrEmpty(textToHash))
            {
                return string.Empty;
            }
            var md5 = new MD5CryptoServiceProvider();
            byte[] textToHashArray = Encoding.Default.GetBytes(textToHash);
            byte[] result = md5.ComputeHash(textToHashArray);
            return BitConverter.ToString(result);
        }

        public void CheckSendEmail()
        {
            if (Duration.TotalDays > SendMailAfterDays && !_warningEmailSent)
            {
                _warningEmailSent = true;
                OnViewIsRunningLong();
            }
        }

        public void SendEmail(DbConfig dbConfig)
        {
            if (Duration.TotalDays > SendMailAfterDays)
            {
                string fromEmail = "";
                try
                {
                    MailMessage message = new MailMessage();
                    fromEmail = ActiveDirectory.GetEmailOfUser() ??
                                ActiveDirectory.GetUserAbbr() + "@avendata.de";
                    message.To.Add("boeminghaus@avendata.de");
                    message.To.Add("m.gerlach@avendata.de");
                    message.To.Add("p.hildebrandt@avendata.de");
                    message.To.Add("d.hamerla@avendata.de");
                    message.CC.Add("b.held@avendata.de");
                    message.CC.Add(fromEmail);
                    message.Attachments.Add(new Attachment(new MemoryStream(Encoding.UTF8.GetBytes(Script)),
                                                           "SqlSkript.txt"));
                    message.Subject = "Vieweinspielung dauert länger als 3 Tage";
                    message.From = new MailAddress(fromEmail);
                    message.Body =
                        "View: " + Name + " (" + Description + ")" + Environment.NewLine +
                        "Server: " + dbConfig.Hostname + Environment.NewLine +
                        "Datenbank: " + dbConfig.DbName + Environment.NewLine +
                        "Bearbeiter: " + User + " (" + ActiveDirectory.GetUserName() + ")" +
                        Environment.NewLine + Environment.NewLine + "Dies ist eine automatisch generierte Email.";
                    SmtpClient smtp = new SmtpClient("brockman.av.local");
                    smtp.Send(message);
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("Konnte keine Email senden von {0}, Fehler: {1}", fromEmail,
                                            ex.Message), ex);
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }

        #endregion methods

        public event EventHandler ViewIsRunningLong;

        private void OnViewIsRunningLong()
        {
            EventHandler viewIsRunningLong = ViewIsRunningLong;
            if (viewIsRunningLong != null) viewIsRunningLong(this, new EventArgs());
        }

        private void Init()
        {
            ProcedureInfo = new ProcedureInfo();
            Id = 0;
            User = ActiveDirectory.GetUserAbbr();
        }

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/

        /*****************************************************************************************************/
    }
}