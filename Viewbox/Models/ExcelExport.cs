using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SystemDb;
using Viewbox.Properties;
using ViewboxDb;

namespace Viewbox.Models
{
	public class ExcelExport
	{
		private const string SEPARATOR = ";";

		private const string INDENT = "   ";

		private const string FORMAT_DATETIME = "dd.MM.yyyy HH:mm:ss";

		private const string FORMAT_DATE = "dd.MM.yyyy";

		private const string FORMAT_TIME = "HH:mm:ss";

		private readonly Stream _outStream;

		private readonly ExportType _exportType;

		private readonly ITableObject _tobj;

		private readonly IOptimization _optimization;

		private readonly IUser _user;

		private readonly ILanguage _language;

		private readonly Dictionary<string, string> _parameters = new Dictionary<string, string>();

		private readonly List<ExportColumn> _columns = new List<ExportColumn>();

		private readonly List<List<object>> _rows = new List<List<object>>();

		public Encoding Encoding { get; set; }

		public bool AllowCSVEscapes { get; set; }

		public Dictionary<string, string> Parameters => _parameters;

		public List<ExportColumn> Columns => _columns;

		public List<List<object>> Rows => _rows;

		public bool WriteOptimization { get; set; }

		public ExcelExport(Stream outStream, ExportType exportType, ITableObject tobj, IOptimization optimization, IUser user, ILanguage language)
		{
			_outStream = outStream;
			_exportType = exportType;
			_tobj = tobj;
			_language = language;
			_optimization = optimization;
			_user = user;
			Encoding = ((exportType == ExportType.GDPdU) ? Encoding.GetEncoding(1252) : Encoding.Unicode);
			AllowCSVEscapes = true;
			WriteOptimization = true;
		}

		private void WriteData(string str, bool escape = true)
		{
			byte[] buffer = ((AllowCSVEscapes && escape) ? Encoding.GetBytes(ToCsvStr(str)) : Encoding.GetBytes(str));
			_outStream.Write(buffer, 0, buffer.Length);
		}

		private void WriteSeparator()
		{
			WriteData(";", escape: false);
		}

		private void WriteNewLine()
		{
			WriteData(Environment.NewLine, escape: false);
		}

		private string ToCsvStr(string str)
		{
			if (str.Contains("\n") && !str.Contains("\r\n"))
			{
				str = str.Replace("\n", Environment.NewLine);
			}
			if (str.Contains(";") || str.Contains(' ') || str.Contains('"') || str.Contains(Environment.NewLine))
			{
				return "\"" + str.Replace("\"", "\"\"") + "\"";
			}
			return str;
		}

		public void DoExport()
		{
			WriteRows();
		}

		public void WriteCSVStart()
		{
			byte[] preamble = Encoding.GetPreamble();
			if (preamble.Length != 0)
			{
				_outStream.Write(preamble, 0, preamble.Length);
			}
			if (Encoding == Encoding.Unicode)
			{
				WriteData("sep=;", escape: false);
				WriteNewLine();
			}
		}

		public void WriteHeader()
		{
			if (ViewboxApplication.FindProperty(_user, "export_description")?.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ?? true)
			{
				WriteType();
				WriteOptimizations();
				WriteParameters();
			}
		}

		private void WriteParameters()
		{
			if (_parameters.Count <= 0)
			{
				return;
			}
			WriteData(Resources.DisplayParameter);
			WriteNewLine();
			foreach (KeyValuePair<string, string> item in _parameters)
			{
				WriteData("   " + item.Key + ": " + item.Value);
				WriteNewLine();
			}
			WriteNewLine();
			WriteNewLine();
		}

		private void WriteOptimizations()
		{
			if (!WriteOptimization)
			{
				return;
			}
			List<Tuple<string, string>> optimizationTexts = _optimization.GetDescriptions(_language, _tobj);
			optimizationTexts.Reverse();
			WriteData(Resources.ChosenOptimizations);
			WriteNewLine();
			foreach (Tuple<string, string> opt in optimizationTexts)
			{
				WriteData("   " + opt.Item1 + ": " + opt.Item2);
				WriteNewLine();
			}
			WriteNewLine();
		}

		private void WriteType()
		{
			ExportType exportType = _exportType;
			WriteData(exportType.ToString() + "-" + Resources.ExportOf + ":");
			WriteNewLine();
			StringBuilder sb = new StringBuilder();
			sb.Append("   ");
			if (_tobj.Type == TableType.View)
			{
				sb.Append(Resources.View);
			}
			if (_tobj.Type == TableType.Table)
			{
				sb.Append(Resources.Table);
			}
			if (_tobj.Type == TableType.Issue)
			{
				sb.Append(Resources.Issue);
			}
			sb.Append(": ");
			sb.Append(_tobj.GetDescription(_language));
			WriteData(sb.ToString());
			WriteNewLine();
			WriteNewLine();
		}

		public void WriteColumns()
		{
			for (int i = 0; i < _columns.Count; i++)
			{
				if (i > 0)
				{
					WriteSeparator();
				}
				WriteData(_columns[i].Name);
			}
			WriteNewLine();
		}

		private void WriteRows()
		{
			foreach (List<object> row in _rows)
			{
				for (int i = 0; i < row.Count; i++)
				{
					if (i > 0)
					{
						WriteSeparator();
					}
					object data = row[i];
					Type dataType = data.GetType();
					switch (_columns[i].Type)
					{
					case SqlType.String:
						if (dataType == typeof(byte[]))
						{
							byte[] array = data as byte[];
							StringBuilder value = new StringBuilder();
							for (int j = 0; j < array.Length; j++)
							{
								if (j > 0)
								{
									value.Append((j % 16 > 0) ? " " : Environment.NewLine);
								}
								value.Append($"{array[j]:X2}");
							}
							WriteData(value.ToString());
						}
						else
						{
							WriteData(data.ToString());
						}
						break;
					case SqlType.Time:
					{
						if (_exportType == ExportType.GDPdU && DateTime.TryParse(data.ToString(), out var toDateTime))
						{
							WriteData(toDateTime.ToString("HH:mm:ss"));
						}
						else
						{
							WriteData(data.ToString());
						}
						break;
					}
					case SqlType.Date:
					{
						if (_exportType == ExportType.GDPdU && DateTime.TryParse(data.ToString(), out var toDateTime2))
						{
							WriteData(toDateTime2.ToString("dd.MM.yyyy"));
						}
						else
						{
							WriteData(data.ToString());
						}
						break;
					}
					case SqlType.DateTime:
					{
						if (_exportType == ExportType.GDPdU && DateTime.TryParse(data.ToString(), out var toDateTime3))
						{
							WriteData(toDateTime3.ToString("dd.MM.yyyy HH:mm:ss"));
							WriteSeparator();
							WriteData(toDateTime3.ToString("dd.MM.yyyy"));
							WriteSeparator();
							WriteData(toDateTime3.ToString("HH:mm:ss"));
						}
						else
						{
							WriteData(data.ToString());
						}
						break;
					}
					case SqlType.Decimal:
					case SqlType.Numeric:
					{
						decimal decValue;
						double doubleValue;
						if (dataType == typeof(decimal))
						{
							WriteData(((decimal)data).ToString());
						}
						else if (dataType == typeof(double))
						{
							WriteData(((double)data).ToString());
						}
						else if (decimal.TryParse(data.ToString(), out decValue))
						{
							WriteData(decValue.ToString());
						}
						else if (double.TryParse(data.ToString(), out doubleValue))
						{
							WriteData(doubleValue.ToString());
						}
						else
						{
							WriteData(data.ToString());
						}
						break;
					}
					default:
						WriteData(data.ToString());
						break;
					}
				}
				WriteNewLine();
			}
			_outStream.Flush();
		}
	}
}
