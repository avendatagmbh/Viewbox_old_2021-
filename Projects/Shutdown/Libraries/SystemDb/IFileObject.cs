using System;

namespace SystemDb
{
	public interface IFileObject
	{
		int Id { get; set; }

		int DirectoryId { get; set; }

		string Name { get; set; }

		int Size { get; set; }

		DateTime Date { get; set; }
	}
}
