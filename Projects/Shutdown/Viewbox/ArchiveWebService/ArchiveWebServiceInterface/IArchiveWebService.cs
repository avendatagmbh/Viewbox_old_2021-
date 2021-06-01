using System.Collections.Generic;
using System.ServiceModel;
using ViewboxDb;
using ViewboxDb.Filters;

namespace ArchiveWebServiceInterface
{
	[ServiceContract]
	public interface IArchiveWebService
	{
		[OperationContract]
		string GetPath(string id, string additional);

		[OperationContract]
		List<Document> GetDocuments(IFilter filter, SortCollection sort, long from, long to, string additional);

		[OperationContract]
		long GetCount(IFilter filter, string additional);

		[OperationContract]
		Dictionary<string, string> GetTypes(string additional);
	}
}
