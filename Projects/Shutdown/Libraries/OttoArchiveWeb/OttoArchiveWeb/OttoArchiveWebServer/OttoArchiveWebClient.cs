using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace OttoArchive.OttoArchiveWebServer
{
	[DebuggerStepThrough]
	[GeneratedCode("System.ServiceModel", "4.0.0.0")]
	public class OttoArchiveWebClient : ClientBase<OttoArchiveWeb>, OttoArchiveWeb
	{
		public OttoArchiveWebClient()
		{
		}

		public OttoArchiveWebClient(string endpointConfigurationName)
			: base(endpointConfigurationName)
		{
		}

		public OttoArchiveWebClient(string endpointConfigurationName, string remoteAddress)
			: base(endpointConfigurationName, remoteAddress)
		{
		}

		public OttoArchiveWebClient(string endpointConfigurationName, EndpointAddress remoteAddress)
			: base(endpointConfigurationName, remoteAddress)
		{
		}

		public OttoArchiveWebClient(Binding binding, EndpointAddress remoteAddress)
			: base(binding, remoteAddress)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		InitResponse OttoArchiveWeb.Init(InitRequest request)
		{
			return base.Channel.Init(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		GetDocumentsResponse OttoArchiveWeb.GetDocuments(GetDocumentsRequest request)
		{
			return base.Channel.GetDocuments(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		GetDocumentResponse OttoArchiveWeb.GetDocument(GetDocumentRequest request)
		{
			return base.Channel.GetDocument(request);
		}

		public void CloseServer()
		{
			base.Channel.CloseServer();
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		GetDescriptorsResponse OttoArchiveWeb.GetDescriptors(GetDescriptorsRequest request)
		{
			return base.Channel.GetDescriptors(request);
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		GetDocumentInfoResponse OttoArchiveWeb.GetDocumentInfo(GetDocumentInfoRequest request)
		{
			return base.Channel.GetDocumentInfo(request);
		}

		public bool Init(string ArchivServerName, string SeratioServerName, string ArchivPort, string SeratioPort, string SysDBName, string DataDbName, string User, string Password, string[] PeriNames, out string errors)
		{
			InitRequest inValue = new InitRequest();
			inValue.ArchivServerName = ArchivServerName;
			inValue.SeratioServerName = SeratioServerName;
			inValue.ArchivPort = ArchivPort;
			inValue.SeratioPort = SeratioPort;
			inValue.SysDBName = SysDBName;
			inValue.DataDbName = DataDbName;
			inValue.User = User;
			inValue.Password = Password;
			inValue.PeriNames = PeriNames;
			InitResponse retVal = ((OttoArchiveWeb)this).Init(inValue);
			errors = retVal.errors;
			return retVal.@out;
		}

		public bool GetDocuments(string search, string descriptorId, bool closeServer, out string errors, out CustomDocument documents)
		{
			GetDocumentsRequest inValue = new GetDocumentsRequest();
			inValue.search = search;
			inValue.descriptorId = descriptorId;
			inValue.closeServer = closeServer;
			GetDocumentsResponse retVal = ((OttoArchiveWeb)this).GetDocuments(inValue);
			errors = retVal.errors;
			documents = retVal.documents;
			return retVal.@out;
		}

		public bool GetDocument(string id, bool closeServer, out string errors, out byte[] binary)
		{
			GetDocumentRequest inValue = new GetDocumentRequest();
			inValue.id = id;
			inValue.closeServer = closeServer;
			GetDocumentResponse retVal = ((OttoArchiveWeb)this).GetDocument(inValue);
			errors = retVal.errors;
			binary = retVal.binary;
			return retVal.@out;
		}

		public bool GetDescriptors(bool closeserver, out string errors, out CustomDescriptor descriptors)
		{
			GetDescriptorsRequest inValue = new GetDescriptorsRequest();
			inValue.closeserver = closeserver;
			GetDescriptorsResponse retVal = ((OttoArchiveWeb)this).GetDescriptors(inValue);
			errors = retVal.errors;
			descriptors = retVal.descriptors;
			return retVal.@out;
		}

		public bool GetDocumentInfo(string documentId, bool closeServer, out string errors, out DocumentInfo infos)
		{
			GetDocumentInfoRequest inValue = new GetDocumentInfoRequest();
			inValue.documentId = documentId;
			inValue.closeServer = closeServer;
			GetDocumentInfoResponse retVal = ((OttoArchiveWeb)this).GetDocumentInfo(inValue);
			errors = retVal.errors;
			infos = retVal.infos;
			return retVal.@out;
		}
	}
}
