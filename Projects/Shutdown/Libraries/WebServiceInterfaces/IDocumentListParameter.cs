namespace WebServiceInterfaces
{
	public interface IDocumentListParameter : IParameter
	{
		IDocumentList output { get; set; }

		IDocumentSearch input { get; set; }
	}
}
