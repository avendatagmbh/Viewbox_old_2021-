namespace SystemDb.Compatibility.Viewbuilder.OptimizationRelated
{
	public class OptimizationGroup
	{
		public IOptimizationGroup CreatedFrom { get; private set; }

		public OptimizationType Type { get; private set; }

		public string ReadableTypeString => Type switch
		{
			OptimizationType.SplitTable => "Buchungskreis", 
			OptimizationType.SortColumn => "Geschäftsjahr", 
			OptimizationType.IndexTable => "Mandant", 
			_ => Type.ToString(), 
		};

		public OptimizationGroup(OptimizationType type, IOptimizationGroup group = null)
		{
			CreatedFrom = group;
			Type = type;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			return obj.GetHashCode() == GetHashCode();
		}

		public override int GetHashCode()
		{
			return (int)Type;
		}
	}
}
