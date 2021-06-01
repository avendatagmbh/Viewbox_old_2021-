using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace OttoArchive.OttoArchiveWebServer
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	[MessageContract(WrapperName = "GetDocumentInfo", WrapperNamespace = "http://DefaultNamespace/OttoArchiveWeb/", IsWrapped = true)]
	public class GetDocumentInfoRequest
	{
		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 1)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public bool closeServer;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 0)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public string documentId;

		public GetDocumentInfoRequest()
		{
		}

		public GetDocumentInfoRequest(string documentId, bool closeServer)
		{
			this.documentId = documentId;
			this.closeServer = closeServer;
		}
	}
}
