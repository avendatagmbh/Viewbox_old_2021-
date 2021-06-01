using System;
using System.Runtime.Serialization;

namespace ArchiveWebServiceInterface
{
	[DataContract]
	public class Document : IDocument
	{
		[DataMember]
		public string Id { get; set; }

		[DataMember]
		public string TypeAbbreviation { get; set; }

		[DataMember]
		public string TypeDescription { get; set; }

		[DataMember]
		public bool HasThumbnail { get; set; }

		[DataMember]
		public string Description { get; set; }

		[DataMember]
		public DateTime Date { get; set; }
	}
}
