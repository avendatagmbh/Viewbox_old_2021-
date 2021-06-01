using System.CodeDom.Compiler;
using System.ServiceModel;

namespace OttoArchive.OttoArchiveWebServer
{
	[GeneratedCode("System.ServiceModel", "4.0.0.0")]
	[ServiceContract(Namespace = "http://DefaultNamespace/OttoArchiveWeb/", ConfigurationName = "OttoArchiveWebServer.OttoArchiveWeb")]
	public interface OttoArchiveWeb
	{
		[OperationContract(Action = "http://DefaultNamespace/OttoArchiveWeb/Init", ReplyAction = "*")]
		[XmlSerializerFormat(SupportFaults = true)]
		[return: MessageParameter(Name = "out")]
		InitResponse Init(InitRequest request);

		[OperationContract(Action = "http://DefaultNamespace/OttoArchiveWeb/GetDocuments", ReplyAction = "*")]
		[XmlSerializerFormat(SupportFaults = true)]
		[return: MessageParameter(Name = "out")]
		GetDocumentsResponse GetDocuments(GetDocumentsRequest request);

		[OperationContract(Action = "http://DefaultNamespace/OttoArchiveWeb/GetDocument", ReplyAction = "*")]
		[XmlSerializerFormat(SupportFaults = true)]
		[return: MessageParameter(Name = "out")]
		GetDocumentResponse GetDocument(GetDocumentRequest request);

		[OperationContract(Action = "http://DefaultNamespace/OttoArchiveWeb/CloseServer", ReplyAction = "*")]
		[XmlSerializerFormat(SupportFaults = true)]
		void CloseServer();

		[OperationContract(Action = "http://DefaultNamespace/OttoArchiveWeb/GetDescriptors", ReplyAction = "*")]
		[XmlSerializerFormat(SupportFaults = true)]
		[return: MessageParameter(Name = "out")]
		GetDescriptorsResponse GetDescriptors(GetDescriptorsRequest request);

		[OperationContract(Action = "http://DefaultNamespace/OttoArchiveWeb/GetDocumentInfo", ReplyAction = "*")]
		[XmlSerializerFormat(SupportFaults = true)]
		[return: MessageParameter(Name = "out")]
		GetDocumentInfoResponse GetDocumentInfo(GetDocumentInfoRequest request);
	}
}
