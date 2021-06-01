using WebServiceInterfaces;

namespace OttoArchive.OttoArchiveWeb
{
	public class DocumentInfoParameter : IDocumentInfoParameter, IParameter
	{
		public IDocument document { get; set; }

		public string Errors { get; set; }

		public bool CloseServer { get; set; }

		public DocumentInfoParameter()
		{
			Errors = "";
			CloseServer = true;
		}
	}
}
