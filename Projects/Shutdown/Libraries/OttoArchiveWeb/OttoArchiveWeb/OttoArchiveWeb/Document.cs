using System.Collections.Generic;
using System.IO;
using System.Xml;
using WebServiceInterfaces;

namespace OttoArchive.OttoArchiveWeb
{
	public class Document : IDocument
	{
		public string Id { get; set; }

		public Dictionary<string, string> Descriptors { get; set; }

		public Document()
		{
			Descriptors = new Dictionary<string, string>();
		}

		public XmlDocument ToXml()
		{
			XmlDocument document = new XmlDocument();
			XmlNode rootNode = document.CreateElement("document");
			document.AppendChild(rootNode);
			XmlNode idNode = document.CreateElement("id");
			idNode.InnerText = Id;
			rootNode.AppendChild(idNode);
			XmlNode descriptorsNode = document.CreateElement("descriptors");
			foreach (string key in Descriptors.Keys)
			{
				XmlNode descriptorNode = document.CreateElement("descriptor");
				XmlAttribute attribute = document.CreateAttribute("key");
				attribute.Value = key;
				descriptorNode.Attributes.Append(attribute);
				descriptorNode.InnerText = Descriptors[key];
				descriptorsNode.AppendChild(descriptorNode);
			}
			rootNode.AppendChild(descriptorsNode);
			return document;
		}

		public string ToXmlString()
		{
			using StringWriter stringWriter = new StringWriter();
			using XmlWriter xmlTextWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
			{
				Indent = true
			});
			ToXml().WriteTo(xmlTextWriter);
			xmlTextWriter.Flush();
			return stringWriter.GetStringBuilder().ToString();
		}
	}
}
