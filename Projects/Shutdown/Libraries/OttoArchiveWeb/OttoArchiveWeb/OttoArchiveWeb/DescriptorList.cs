using System.Collections.Generic;
using WebServiceInterfaces;

namespace OttoArchive.OttoArchiveWeb
{
	public class DescriptorList : IDescriptorList
	{
		List<IDescriptor> IDescriptorList.Descriptors { get; set; }
	}
}
