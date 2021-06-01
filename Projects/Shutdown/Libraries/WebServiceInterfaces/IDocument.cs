using System.Collections.Generic;
using System.Xml;

namespace WebServiceInterfaces
{
	public interface IDocument
	{
		string Id { get; set; }

		Dictionary<string, string> Descriptors { get; set; }

		XmlDocument ToXml();

		string ToXmlString();
	}
}
