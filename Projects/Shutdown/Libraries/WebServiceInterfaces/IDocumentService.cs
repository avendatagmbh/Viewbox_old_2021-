namespace WebServiceInterfaces
{
	public interface IDocumentService
	{
		bool Init(IParameter parameter);

		bool GetDocumentList(IDocumentListParameter parameter);

		bool GetDocument(IDocumentParameter parameter);

		bool GetDocumentInfo(IDocumentInfoParameter parameter);

		void CloseContext();

		bool GetDescriptors(IDescriptorListParameter parameter);
	}
}
