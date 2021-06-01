using System;
using System.Collections.Generic;
using System.Linq;

namespace ViewboxBusiness.Common
{
	public class ObjectType
	{
		public Dictionary<string, string> LangToDescription { get; set; }

		public string Name { get; set; }

		public ObjectType(string name)
		{
			LangToDescription = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Name = name;
		}

		public string GetDescription()
		{
			if (LangToDescription.Count == 0)
			{
				return Name;
			}
			return LangToDescription.Values.FirstOrDefault();
		}
	}
}
