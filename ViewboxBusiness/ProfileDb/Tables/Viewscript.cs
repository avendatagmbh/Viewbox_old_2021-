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
using DbAccess.Attributes;
using DbAccess.Structures;
using log4net;
using Utils;
using ViewboxBusiness.Common;
using ViewboxBusiness.Structures;

namespace ViewboxBusiness.ProfileDb.Tables
{
	[DbTable("viewscripts")]
	public class Viewscript : TableBase, IJobInfo
	{
		private const string TABLENAME = "viewscripts";

		private const int SendMailAfterDays = 3;

		private readonly ILog _log = LogHelper.GetLogger();

		private static readonly ViewscriptStatesToStringConverter StateConverter = new ViewscriptStatesToStringConverter();

		private string _error;

		private string _errorSql;

		private string _errorSqlPrefix;

		private string _errorSqlSuffix;

		private FileInfo _fileInfo;

		private string _hash;

		private string _indizes;

		private bool _isChecked;

		private bool _isHovered;

		private string _parsingError;

		private bool? _parsingState;

		private string _script;

		private string _sql;

		private ViewscriptStates _state;

		private string _tables;

		private string _warnings;

		private bool _warningEmailSent;

		private ViewInfo _viewInfo;

		private readonly Stopwatch _sw = new Stopwatch();

		private DateTime _creationTimestamp;

		private TimeSpan _duration;

		[DbColumn("user", AllowDbNull = false, Length = 64)]
		public string User { get; set; }

		[DbColumn("fileName", AllowDbNull = false, Length = 2048)]
		public string FileName { get; set; }

		public string Description => ViewInfo.Description;

		[DbColumn("isChecked", AllowDbNull = false)]
		public bool IsChecked
		{
			get
			{
				return _isChecked;
			}
			set
			{
				if (_isChecked != value)
				{
					_isChecked = value;
					OnPropertyChanged("IsChecked");
				}
			}
		}

		[DbColumn("error", AllowDbNull = false, Length = 100000)]
		public string LastError
		{
			get
			{
				return _error;
			}
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

		[DbColumn("parsingState", AllowDbNull = true)]
		public bool? ParsingState
		{
			get
			{
				return _parsingState;
			}
			set
			{
				if (_parsingState != value)
				{
					_parsingState = value;
					OnPropertyChanged("ParsingState");
				}
			}
		}

		[DbColumn("parsingError", AllowDbNull = true)]
		public string ParsingError
		{
			get
			{
				return _parsingError;
			}
			set
			{
				if (_parsingError != value)
				{
					_parsingError = value;
					OnPropertyChanged("ParsingError");
				}
			}
		}

		[DbColumn("sql", AllowDbNull = false, Length = 100000)]
		public string Sql
		{
			get
			{
				return _sql;
			}
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
			get
			{
				return _warnings;
			}
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

		[DbColumn("tables", AllowDbNull = false, Length = 50000)]
		public string Tables
		{
			get
			{
				return _tables;
			}
			set
			{
				if (_tables != value)
				{
					_tables = value;
					OnPropertyChanged("Tables");
				}
			}
		}

		[DbColumn("hash", AllowDbNull = false, Length = 2048)]
		public string Hash
		{
			get
			{
				return _hash;
			}
			set
			{
				if (_hash != value)
				{
					_hash = value;
					OnPropertyChanged("Hash");
				}
			}
		}

		[DbColumn("indizes", AllowDbNull = true, Length = 50000)]
		public string Indizes
		{
			get
			{
				return _indizes;
			}
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
			get
			{
				if (string.IsNullOrEmpty(LastError))
				{
					return string.IsNullOrEmpty(Warnings);
				}
				return false;
			}
		}

		[DbColumn("name", AllowDbNull = false, Length = 256)]
		public string Name { get; set; }

		[DbColumn("state", AllowDbNull = false)]
		public ViewscriptStates State
		{
			get
			{
				return _state;
			}
			set
			{
				if (_state != value)
				{
					_state = value;
					OnPropertyChanged("State");
				}
			}
		}

		public string Script
		{
			get
			{
				return _script;
			}
			set
			{
				if (_script != value)
				{
					_script = value;
					OnPropertyChanged("Script");
				}
			}
		}

		public string FileTimestampDisplayString
		{
			get
			{
				if (FileInfo == null)
				{
					return string.Empty;
				}
				return FileInfo.LastWriteTime.ToString(CultureInfo.InvariantCulture);
			}
		}

		public FileInfo FileInfo
		{
			get
			{
				return _fileInfo;
			}
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

		public bool IsHovered
		{
			get
			{
				return _isHovered;
			}
			set
			{
				if (_isHovered != value)
				{
					_isHovered = value;
					OnPropertyChanged("IsHovered");
				}
			}
		}

		public string StateDisplayString => (string)StateConverter.Convert(State, typeof(string), null, CultureInfo.CurrentUICulture);

		public string ErrorSqlPrefix
		{
			get
			{
				return _errorSqlPrefix;
			}
			set
			{
				if (_errorSqlPrefix != value)
				{
					_errorSqlPrefix = value;
					OnPropertyChanged("ErrorSqlPrefix");
				}
			}
		}

		public string ErrorSql
		{
			get
			{
				return _errorSql;
			}
			set
			{
				if (_errorSql != value)
				{
					_errorSql = value;
					OnPropertyChanged("ErrorSql");
				}
			}
		}

		public string ErrorSqlSuffix
		{
			get
			{
				return _errorSqlSuffix;
			}
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
			get
			{
				return _viewInfo;
			}
			set
			{
				if (_viewInfo != value)
				{
					_viewInfo = value;
					OnPropertyChanged("");
				}
			}
		}

		public bool HasStoredProcedure => !string.IsNullOrEmpty(ViewInfo.ProcedureName);

		public ProcedureInfo ProcedureInfo { get; set; }

		public DateTime CreationTimestamp
		{
			get
			{
				return _creationTimestamp;
			}
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

		public TimeSpan Duration
		{
			get
			{
				return _duration;
			}
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

		public string DurationDisplayString => StringUtils.FormatTimeSpan(Duration);

		public event EventHandler ViewIsRunningLong;

		public Viewscript()
		{
			ViewInfo = new ViewInfo();
			Init();
		}

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

		public void StartStopwatch()
		{
			_sw.Reset();
			_sw.Start();
			_warningEmailSent = false;
			new Thread(UpdateDuration).Start();
		}

		public void StopStopwatch()
		{
			CreationTimestamp = DateTime.Now;
			_sw.Stop();
			Duration = _sw.Elapsed;
		}

		private void UpdateDuration()
		{
			while (_sw.IsRunning)
			{
				Duration = _sw.Elapsed;
				Thread.Sleep(500);
				CheckSendEmail();
			}
		}

		internal static List<Viewscript> Load(DatabaseBase conn, bool onlyFromUser = true)
		{
			if (onlyFromUser)
			{
				return conn.DbMapping.Load<Viewscript>(conn.Enquote("user") + "=" + conn.GetSqlString(ActiveDirectory.GetUserAbbr()));
			}
			return conn.DbMapping.Load<Viewscript>();
		}

		public void CheckAll()
		{
			try
			{
				if (!base.ProjectDb.ConnectionManager.IsDisposed)
				{
					using DatabaseBase conn = base.ProjectDb.ConnectionManager.GetConnection();
					conn.ExecuteNonQuery("UPDATE " + conn.Enquote("viewscripts") + " SET " + conn.Enquote("isChecked") + "= 1");
				}
			}
			catch
			{
			}
		}

		public void UncheckAll()
		{
			try
			{
				if (!base.ProjectDb.ConnectionManager.IsDisposed)
				{
					using DatabaseBase conn = base.ProjectDb.ConnectionManager.GetConnection();
					conn.ExecuteNonQuery("UPDATE " + conn.Enquote("viewscripts") + " SET " + conn.Enquote("isChecked") + "= 0");
				}
			}
			catch
			{
			}
		}

		private void ParseErrorSql()
		{
			if (Sql == null)
			{
				return;
			}
			string[] sqlLines = Sql.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
			if (LastError.ToLower().Contains("line "))
			{
				if (!int.TryParse(Regex.Match(LastError.Substring(LastError.ToLower().IndexOf("line ", StringComparison.Ordinal)), "([0-9]+)").Groups[1].Value, out var lineNumber))
				{
					ErrorSqlPrefix = Sql;
					ErrorSqlSuffix = string.Empty;
					ErrorSql = string.Empty;
				}
				if (lineNumber - 1 > sqlLines.Length)
				{
					ErrorSqlPrefix = Sql;
					ErrorSqlSuffix = string.Empty;
					ErrorSql = string.Empty;
				}
				else
				{
					SetErrorSqlParts(lineNumber, sqlLines);
				}
			}
			else if (Regex.IsMatch(LastError, "Unknown column '(.+)' in 'field list'", RegexOptions.IgnoreCase))
			{
				string columnName = Regex.Match(LastError, "Unknown column '(.+)' in 'field list'", RegexOptions.IgnoreCase).Groups[1].Value;
				for (int i = 0; i < sqlLines.Length; i++)
				{
					if (sqlLines[i].Contains(columnName))
					{
						SetErrorSqlParts(i, sqlLines);
						break;
					}
				}
			}
			else
			{
				ErrorSqlPrefix = Sql;
				ErrorSqlSuffix = string.Empty;
				ErrorSql = string.Empty;
			}
		}

		private void SetErrorSqlParts(int lineNumber, string[] sqlLines)
		{
			if (lineNumber >= 0 && lineNumber < sqlLines.Length)
			{
				ErrorSql = sqlLines[lineNumber];
			}
			int actLines = 0;
			int curLineNumber = lineNumber - 1;
			ErrorSqlPrefix = string.Empty;
			while (curLineNumber > 0 && actLines < 3 && curLineNumber < sqlLines.Length)
			{
				actLines++;
				if (ErrorSqlPrefix.Length > 0)
				{
					ErrorSqlPrefix = Environment.NewLine + ErrorSqlPrefix;
				}
				ErrorSqlPrefix = sqlLines[curLineNumber] + ErrorSqlPrefix;
				curLineNumber--;
			}
			if (curLineNumber < sqlLines.Length)
			{
				ErrorSqlPrefix = "..." + Environment.NewLine + ErrorSqlPrefix;
			}
			ErrorSqlSuffix = string.Empty;
			actLines = 0;
			for (curLineNumber = lineNumber + 1; curLineNumber < sqlLines.Length; curLineNumber++)
			{
				if (actLines >= 3)
				{
					break;
				}
				actLines++;
				if (ErrorSqlSuffix.Length > 0)
				{
					ErrorSqlSuffix += Environment.NewLine;
				}
				ErrorSqlSuffix += sqlLines[curLineNumber];
			}
			if (curLineNumber < sqlLines.Length)
			{
				ErrorSqlSuffix = ErrorSqlSuffix + Environment.NewLine + "...";
			}
		}

		public void AddWarning(string warning)
		{
			if (Warnings.Length > 0)
			{
				Warnings += Environment.NewLine;
			}
			Warnings += warning;
		}

		public Dictionary<string, long> GetTables()
		{
			Dictionary<string, long> output = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
			using XmlReader reader = XmlReader.Create(new StringReader(Tables));
			while (reader.Read())
			{
				if (reader.Name == "Table")
				{
					output.Add(reader.GetAttribute("Name"), Convert.ToInt64(reader.GetAttribute("Complexity")));
				}
			}
			return output;
		}

		public void SetTables(Dictionary<string, long> tables)
		{
			using MemoryStream memStream = new MemoryStream();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "    ",
				Encoding = Encoding.Default
			};
			using XmlWriter writer = XmlWriter.Create(memStream, settings);
			writer.WriteStartDocument();
			writer.WriteStartElement("Tables");
			foreach (KeyValuePair<string, long> t in tables)
			{
				writer.WriteStartElement("Table");
				writer.WriteAttributeString("Name", t.Key);
				writer.WriteAttributeString("Complexity", t.Value.ToString(CultureInfo.InvariantCulture));
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Flush();
			memStream.Flush();
			memStream.Position = 0L;
			StreamReader reader = new StreamReader(memStream);
			Tables = reader.ReadToEnd();
		}

		public List<Index> GetIndizes()
		{
			List<Index> output = new List<Index>();
			using XmlReader reader = XmlReader.Create(new StringReader(Indizes));
			while (reader.Read())
			{
				if (!(reader.Name == "Table"))
				{
					continue;
				}
				string table = reader.GetAttribute("Name");
				XmlReader sub = reader.ReadSubtree();
				while (sub.Read())
				{
					if (!(reader.Name == "Index"))
					{
						continue;
					}
					string attribute = reader.GetAttribute("Fields");
					if (attribute != null)
					{
						Index index = new Index(table, attribute.Split(';').ToList());
						if (!output.Contains(index))
						{
							output.Add(index);
						}
					}
				}
			}
			return output;
		}

		public void SetIndizes(List<Index> indizes)
		{
			using MemoryStream memStream = new MemoryStream();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				IndentChars = "    ",
				Encoding = Encoding.Default
			};
			using XmlWriter writer = XmlWriter.Create(memStream, settings);
			writer.WriteStartDocument();
			writer.WriteStartElement("Indizes");
			foreach (IGrouping<string, Index> index in from i in indizes
				group i by i.Table)
			{
				writer.WriteStartElement("Table");
				writer.WriteAttributeString("Name", index.Key);
				foreach (Index j in index)
				{
					writer.WriteStartElement("Index");
					writer.WriteAttributeString("Fields", string.Join(";", j.Columns));
					writer.WriteEndElement();
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Flush();
			memStream.Flush();
			memStream.Position = 0L;
			StreamReader reader = new StreamReader(memStream);
			Indizes = reader.ReadToEnd();
		}

		public string GetHash(string textToHash)
		{
			if (string.IsNullOrEmpty(textToHash))
			{
				return string.Empty;
			}
			MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
			byte[] textToHashArray = Encoding.Default.GetBytes(textToHash);
			return BitConverter.ToString(mD5CryptoServiceProvider.ComputeHash(textToHashArray));
		}

		public void CheckSendEmail()
		{
			if (Duration.TotalDays > 3.0 && !_warningEmailSent)
			{
				_warningEmailSent = true;
				OnViewIsRunningLong();
			}
		}

		public void SendEmail(DbConfig dbConfig)
		{
			if (Duration.TotalDays > 3.0)
			{
				string fromEmail = "";
				try
				{
					MailMessage message = new MailMessage();
					fromEmail = ActiveDirectory.GetEmailOfUser() ?? (ActiveDirectory.GetUserAbbr() + "@avendata.de");
					message.To.Add("boeminghaus@avendata.de");
					message.To.Add("m.gerlach@avendata.de");
					message.To.Add("p.hildebrandt@avendata.de");
					message.To.Add("d.hamerla@avendata.de");
					message.CC.Add("b.held@avendata.de");
					message.CC.Add(fromEmail);
					message.Attachments.Add(new Attachment(new MemoryStream(Encoding.UTF8.GetBytes(Script)), "SqlSkript.txt"));
					message.Subject = "Vieweinspielung dauert länger als 3 Tage";
					message.From = new MailAddress(fromEmail);
					message.Body = "View: " + Name + " (" + Description + ")" + Environment.NewLine + "Server: " + dbConfig.Hostname + Environment.NewLine + "Datenbank: " + dbConfig.DbName + Environment.NewLine + "Bearbeiter: " + User + " (" + ActiveDirectory.GetUserName() + ")" + Environment.NewLine + Environment.NewLine + "Dies ist eine automatisch generierte Email.";
					new SmtpClient().Send(message);
				}
				catch (Exception ex)
				{
					_log.Error($"Konnte keine Email senden von {fromEmail}, Fehler: {ex.Message}", ex);
				}
			}
		}

		public override string ToString()
		{
			return Name;
		}

		private void OnViewIsRunningLong()
		{
			this.ViewIsRunningLong?.Invoke(this, new EventArgs());
		}

		private void Init()
		{
			ProcedureInfo = new ProcedureInfo();
			base.Id = 0;
			User = ActiveDirectory.GetUserAbbr();
		}
	}
}
