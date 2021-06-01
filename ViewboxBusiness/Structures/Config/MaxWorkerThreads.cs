using System.Globalization;

namespace ViewboxBusiness.Structures.Config
{
	public class MaxWorkerThreads
	{
		public int Value { get; set; }

		public string DisplayString
		{
			get
			{
				if (Value == 1)
				{
					return "1 Thread verwenden";
				}
				return Value.ToString(CultureInfo.InvariantCulture) + " Threads verwenden";
			}
		}

		public MaxWorkerThreads(int value)
		{
			Value = value;
		}
	}
}
