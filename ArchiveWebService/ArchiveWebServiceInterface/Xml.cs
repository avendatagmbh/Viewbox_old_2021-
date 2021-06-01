using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace ArchiveWebServiceInterface
{
	public static class Xml
	{
		public static Dictionary<string, List<string>> ReadXmlString(string xml)
		{
			using XmlReader xmlReader = XmlReader.Create(new StringReader(xml));
			Dictionary<string, List<string>> dict = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
			while (xmlReader.Read())
			{
				if (xmlReader.Depth > 0 && xmlReader.HasAttributes)
				{
					if (dict.ContainsKey(xmlReader.Name))
					{
						dict[xmlReader.Name].Add(xmlReader.GetAttribute("Value"));
						continue;
					}
					dict.Add(xmlReader.Name, new List<string> { xmlReader.GetAttribute("Value") });
				}
			}
			return dict;
		}

		public static string WriteXmlString(Dictionary<string, List<string>> additional)
		{
			StringBuilder output = new StringBuilder();
			using (XmlWriter xmlWriter = XmlWriter.Create(output))
			{
				xmlWriter.WriteStartDocument();
				xmlWriter.WriteStartElement("Attributes");
				foreach (KeyValuePair<string, List<string>> param in additional)
				{
					foreach (string p in param.Value)
					{
						xmlWriter.WriteStartElement(param.Key);
						xmlWriter.WriteAttributeString("Value", p);
						xmlWriter.WriteEndElement();
					}
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndDocument();
			}
			return output.ToString();
		}
	}
}
