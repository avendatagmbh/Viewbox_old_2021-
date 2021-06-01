using System;

namespace SystemDb
{
	public interface IObjectTypeRelations : ICloneable
	{
		int Id { get; }

		int Object_Id { get; }

		int Ref_Id { get; }
	}
}
