using System;

namespace ArchiveWebServiceInterface
{
	public interface IDocument
	{
		string Id { get; set; }

		string TypeAbbreviation { get; set; }

		string TypeDescription { get; set; }

		bool HasThumbnail { get; set; }

		string Description { get; set; }

		DateTime Date { get; set; }
	}
}
