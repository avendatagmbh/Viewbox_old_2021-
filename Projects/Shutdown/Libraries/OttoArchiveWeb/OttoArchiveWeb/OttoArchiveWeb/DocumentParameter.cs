using WebServiceInterfaces;

namespace OttoArchive.OttoArchiveWeb
{
	public class DocumentParameter : IDocumentParameter, IParameter
	{
		public IDocument input { get; set; }

		public IDocumentBinary output { get; set; }

		public string Errors { get; set; }

		public bool CloseServer { get; set; }

		public DocumentParameter()
		{
			Errors = "";
			output = new DocumentBinary();
			CloseServer = true;
		}
	}
}
