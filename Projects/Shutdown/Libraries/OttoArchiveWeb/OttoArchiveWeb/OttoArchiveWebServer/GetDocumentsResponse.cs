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
	[MessageContract(WrapperName = "GetDocumentsResponse", WrapperNamespace = "http://DefaultNamespace/OttoArchiveWeb/", IsWrapped = true)]
	public class GetDocumentsResponse
	{
		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 2)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public CustomDocument documents;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 1)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public string errors;

		[MessageBodyMember(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", Order = 0)]
		[XmlElement(Form = XmlSchemaForm.Unqualified)]
		public bool @out;

		public GetDocumentsResponse()
		{
		}

		public GetDocumentsResponse(bool @out, string errors, CustomDocument documents)
		{
			this.@out = @out;
			this.errors = errors;
			this.documents = documents;
		}
	}
}
