namespace WebServiceInterfaces
{
	public interface IDocumentParameter : IParameter
	{
		IDocument input { get; set; }

		IDocumentBinary output { get; set; }
	}
}
