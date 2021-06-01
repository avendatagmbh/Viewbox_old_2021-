using System.Collections.Generic;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("directories", ForceInnoDb = true)]
	public class DirectoryObject : IDirectoryObject
	{
		[DbColumn("id")]
		public int Id { get; set; }

		[DbColumn("parent_id")]
		public int ParentId { get; set; }

		[DbColumn("name")]
		public string Name { get; set; }

		public List<IDirectoryObject> Children { get; set; }

		public IDirectoryObject Clone()
		{
			return new DirectoryObject
			{
				Id = Id,
				ParentId = ParentId,
				Name = Name,
				Children = Children
			};
		}
	}
}
