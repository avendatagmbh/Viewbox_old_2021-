namespace WebServiceInterfaces
{
	public interface IParameter
	{
		string Errors { get; set; }

		bool CloseServer { get; set; }
	}
}
