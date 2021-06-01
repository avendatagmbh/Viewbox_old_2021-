using System;

namespace ViewboxDb
{
	[Serializable]
	public class ValueListElement
	{
		public int Id { get; set; }

		public object Value { get; set; }

		public string Description { get; set; }
	}
}
