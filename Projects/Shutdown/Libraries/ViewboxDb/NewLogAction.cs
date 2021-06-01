using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using SystemDb;
using DbAccess.Attributes;

namespace ViewboxDb
{
	[DbTable("new_log_action")]
	public class NewLogAction
	{
		private readonly Dictionary<string, string> _description = new Dictionary<string, string>();

		public NewLogActionCollection SubActions = new NewLogActionCollection();

		private Dictionary<string, object> _actionParameters = new Dictionary<string, object>();

		private IUser _user;

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("user_id")]
		public int UserId { get; set; }

		public IUser User
		{
			get
			{
				return _user;
			}
			set
			{
				_user = value;
				UserId = ((_user != null) ? _user.Id : 0);
			}
		}

		[DbColumn("timestamp")]
		public DateTime Timestamp { get; set; }

		[DbColumn("action_controller")]
		public NewLogActionControllers ActionController { get; set; }

		public bool CheckCharacterCode { get; set; }

		[DbColumn("query_string", Length = 10000)]
		public string QueryString { get; set; }

		public Dictionary<string, object> ActionParameters
		{
			get
			{
				return _actionParameters;
			}
			set
			{
				if (value.Count != 0)
				{
					_actionParameters = value;
					QueryString = GetSerializedParameter(_actionParameters);
				}
			}
		}

		[DbColumn("parent_id")]
		public int Parentid { get; set; }

		public string Controller { get; set; }

		public bool PageChanged { get; set; }

		public bool SizeChanged { get; set; }

		public bool SearchChanged { get; set; }

		public bool Rightsmode { get; set; }

		public Dictionary<string, string> Description => _description;

		public string GetSerializedParameter(IDictionary<string, object> actionParameters)
		{
			using MemoryStream memStream = new MemoryStream();
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "    ";
			settings.Encoding = Encoding.Unicode;
			using XmlWriter writer = XmlWriter.Create(memStream, settings);
			toXml(writer, actionParameters);
			writer.Flush();
			memStream.Flush();
			memStream.Position = 0L;
			return new StreamReader(memStream).ReadToEnd();
		}

		private void toXml(XmlWriter writer, IDictionary<string, object> actionParameters)
		{
			writer.WriteStartDocument();
			writer.WriteStartElement("Parameters");
			foreach (KeyValuePair<string, object> param in actionParameters)
			{
				writer.WriteStartElement("ActionParameter");
				writer.WriteElementString("Key", param.Key);
				writer.WriteStartElement("Value");
				writer.WriteAttributeString("type", (param.Value != null) ? param.Value.GetType().ToString() : "null");
				if (param.Value != null && param.Value.GetType() == typeof(SortCollection))
				{
					foreach (Sort sort in param.Value as SortCollection)
					{
						writer.WriteStartElement("Sort");
						writer.WriteElementString("cid", sort.cid.ToString());
						writer.WriteElementString("dir", sort.dir.ToString());
						writer.WriteEndElement();
					}
				}
				else if (param.Value != null && param.Value.GetType() == typeof(string[]))
				{
					writer.WriteRaw((param.Value != null) ? string.Join(",", (string[])param.Value) : "null");
				}
				else if (param.Value != null && param.Value.GetType() == typeof(List<AggregationFunction>))
				{
					writer.WriteRaw((param.Value != null) ? string.Join(",", ((List<AggregationFunction>)param.Value).ToArray()) : "null");
				}
				else if (param.Value != null && param.Value.GetType() == typeof(List<int>))
				{
					writer.WriteRaw((param.Value != null) ? string.Join(",", ((List<int>)param.Value).ToArray()) : "null");
				}
				else if (param.Value != null && param.Value.GetType() == typeof(JoinColumnsCollection))
				{
					foreach (JoinColumns cols2 in param.Value as JoinColumnsCollection)
					{
						writer.WriteStartElement("Join");
						writer.WriteElementString("col1", cols2.Column1.ToString());
						writer.WriteElementString("col2", cols2.Column2.ToString());
						writer.WriteEndElement();
					}
				}
				else if (param.Value != null && param.Value.GetType() == typeof(List<JoinColumns>))
				{
					foreach (JoinColumns cols in param.Value as List<JoinColumns>)
					{
						writer.WriteStartElement("Relation");
						writer.WriteElementString("col1", cols.Column1.ToString());
						writer.WriteElementString("col2", cols.Column2.ToString());
						writer.WriteEndElement();
					}
				}
				else if (CheckCharacterCode)
				{
					string checkedValue = string.Empty;
					if (param.Value != null)
					{
						checkedValue = CheckCharCodes(param.Value.ToString());
					}
					writer.WriteRaw((param.Value != null) ? checkedValue : "null");
				}
				else
				{
					writer.WriteRaw((param.Value != null) ? param.Value.ToString() : "null");
				}
				writer.WriteEndElement();
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		public int GetCharacterCode(char character)
		{
			return BitConverter.ToInt32(new UTF32Encoding().GetBytes(character.ToString().ToCharArray()), 0);
		}

		private string CheckCharCodes(string value)
		{
			string result = string.Empty;
			for (int i = 0; i < value.Length; i++)
			{
				int c = GetCharacterCode(value[i]);
				result += ((c > 65000 || c < 33) ? "!" : value[i].ToString());
			}
			return result;
		}
	}
}
