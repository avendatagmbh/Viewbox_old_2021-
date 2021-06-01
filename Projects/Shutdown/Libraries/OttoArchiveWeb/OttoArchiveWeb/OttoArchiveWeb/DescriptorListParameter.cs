using WebServiceInterfaces;

namespace OttoArchive.OttoArchiveWeb
{
	public class DescriptorListParameter : IDescriptorListParameter, IParameter
	{
		public string Errors { get; set; }

		public IDescriptorList output { get; set; }

		public bool CloseServer { get; set; }

		public DescriptorListParameter()
		{
			Errors = "";
			output = new DescriptorList();
			CloseServer = true;
		}
	}
}
