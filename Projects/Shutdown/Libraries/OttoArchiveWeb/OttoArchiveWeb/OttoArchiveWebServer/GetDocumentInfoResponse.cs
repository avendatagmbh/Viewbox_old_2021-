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
	[MessageContract(WrapperName = "GetDocumentInfoResponse", WrapperNamespace = "http://DefaultNamespace/OttoArchiveWeb/", IsWrapped = true)]
	public class GetDocumentInfoResponse
	{
		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 1)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public string errors;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 2)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public DocumentInfo infos;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 0)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public bool @out;

		public GetDocumentInfoResponse()
		{
		}

		public GetDocumentInfoResponse(bool @out, string errors, DocumentInfo infos)
		{
			this.@out = @out;
			this.errors = errors;
			this.infos = infos;
		}
	}
}
