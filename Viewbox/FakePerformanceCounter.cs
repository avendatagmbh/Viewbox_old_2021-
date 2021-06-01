namespace Viewbox
{
	internal class FakePerformanceCounter : IPerformanceCounter
	{
		public float NextValue()
		{
			return -1f;
		}
	}
}
