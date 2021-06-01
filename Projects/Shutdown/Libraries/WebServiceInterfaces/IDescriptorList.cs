using System.Collections.Generic;

namespace WebServiceInterfaces
{
	public interface IDescriptorList
	{
		List<IDescriptor> Descriptors { get; set; }
	}
}
