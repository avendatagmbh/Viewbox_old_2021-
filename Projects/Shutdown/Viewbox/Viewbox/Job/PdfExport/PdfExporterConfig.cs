using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Viewbox.Job.PdfExport
{
	public static class PdfExporterConfig
	{
		private static readonly PdfExporterConfigElement DefaultExporter = new PdfExporterConfigElement
		{
			Exporter = "Viewbox.Job.PdfExport.General.LegacyPdfExporter"
		};

		private static readonly IDictionary<string, PdfExporterConfigElement> ConfigItems = new Dictionary<string, PdfExporterConfigElement>();

		public static void Load(string configFileName)
		{
			if (File.Exists(configFileName))
			{
				ReadData(LoadXmlDocument(configFileName));
			}
		}

		private static XmlDocument LoadXmlDocument(string configFileName)
		{
			XmlDocument configXmlDocument = new XmlDocument();
			configXmlDocument.Load(configFileName);
			return configXmlDocument;
		}

		private static void ReadData(XmlDocument configXmlDocument)
		{
			ConfigItems.Clear();
			XmlNodeList databaseNodeList = configXmlDocument.GetElementsByTagName("database");
			foreach (XmlNode databaseNode in databaseNodeList)
			{
				if (databaseNode.NodeType == XmlNodeType.Comment)
				{
					continue;
				}
				string database = GetAttributeValue(databaseNode, "name");
				foreach (XmlNode reportNode in databaseNode.ChildNodes)
				{
					if (reportNode.NodeType != XmlNodeType.Comment)
					{
						string report = GetAttributeValue(reportNode, "name");
						string exporter = GetAttributeValue(reportNode, "exporter");
						string extinfo = GetAttributeValue(reportNode, "extinfo");
						string extinfo2 = GetAttributeValue(reportNode, "extinfo2");
						string extinfo3 = GetAttributeValue(reportNode, "extinfo3");
						string extinfo4 = GetAttributeValue(reportNode, "extinfo4");
						string extinfo5 = GetAttributeValue(reportNode, "extinfo5");
						AddToConfigItems(CreateKey(database, report), new PdfExporterConfigElement
						{
							Exporter = exporter,
							ExtInfo = extinfo,
							ExtInfo2 = extinfo2,
							ExtInfo3 = extinfo3,
							ExtInfo4 = extinfo4,
							ExtInfo5 = extinfo5
						});
					}
				}
			}
		}

		private static void AddToConfigItems(string key, PdfExporterConfigElement value)
		{
			if (ConfigItems.ContainsKey(key))
			{
				ConfigItems[key] = value;
			}
			else
			{
				ConfigItems.Add(key, value);
			}
		}

		private static string GetAttributeValue(XmlNode node, string attrName)
		{
			string value = string.Empty;
			if (node.Attributes != null)
			{
				XmlAttribute attr = node.Attributes[attrName];
				value = ((attr != null) ? attr.Value : string.Empty);
			}
			return value;
		}

		private static string CreateKey(string database, string report)
		{
			return $"{database}-{report}";
		}

		public static PdfExporterConfigElement GetExporter(string database, string report)
		{
			string key = CreateKey(database, report);
			return ConfigItems.Keys.Contains(key) ? ConfigItems[key] : DefaultExporter;
		}
	}
}
