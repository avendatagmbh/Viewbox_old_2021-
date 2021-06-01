using System.Collections.Generic;

namespace SystemDb
{
	public interface IDirectoryObject
	{
		int Id { get; set; }

		int ParentId { get; set; }

		string Name { get; set; }

		List<IDirectoryObject> Children { get; set; }

		IDirectoryObject Clone();
	}
}
