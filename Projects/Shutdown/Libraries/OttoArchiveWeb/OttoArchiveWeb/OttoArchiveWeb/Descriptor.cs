using WebServiceInterfaces;

namespace OttoArchive.OttoArchiveWeb
{
	public class Descriptor : IDescriptor
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public override string ToString()
		{
			return Id + " - " + Name;
		}
	}
}
