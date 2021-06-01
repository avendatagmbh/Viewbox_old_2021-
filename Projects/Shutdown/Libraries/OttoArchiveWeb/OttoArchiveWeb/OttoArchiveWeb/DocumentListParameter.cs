using WebServiceInterfaces;

namespace OttoArchive.OttoArchiveWeb
{
	public class DocumentListParameter : IDocumentListParameter, IParameter
	{
		public string Errors { get; set; }

		public IDocumentList output { get; set; }

		public IDocumentSearch input { get; set; }

		public bool CloseServer { get; set; }

		public DocumentListParameter()
		{
			Errors = "";
			output = new DocumentList();
			CloseServer = true;
		}
	}
}
