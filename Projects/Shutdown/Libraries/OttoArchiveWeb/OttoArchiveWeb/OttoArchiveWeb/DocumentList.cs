using System.Collections.Generic;
using WebServiceInterfaces;

namespace OttoArchive.OttoArchiveWeb
{
	public class DocumentList : IDocumentList
	{
		List<IDocument> IDocumentList.DocumentList { get; set; }
	}
}
