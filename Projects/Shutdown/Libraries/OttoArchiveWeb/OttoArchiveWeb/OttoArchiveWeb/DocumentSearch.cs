using WebServiceInterfaces;

namespace OttoArchive.OttoArchiveWeb
{
	public class DocumentSearch : IDocumentSearch
	{
		public string Search { get; set; }

		public string DescriptorId { get; set; }
	}
}
