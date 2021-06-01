namespace WebServiceInterfaces
{
	public interface IDocumentInfoParameter : IParameter
	{
		IDocument document { get; set; }
	}
}
