using System.Diagnostics;

namespace Viewbox
{
	internal class EncapsulatedPerformanceCounter : IPerformanceCounter
	{
		private PerformanceCounter _pc;

		public EncapsulatedPerformanceCounter(PerformanceCounter pc)
		{
			_pc = pc;
		}

		public float NextValue()
		{
			return _pc.NextValue();
		}
	}
}
