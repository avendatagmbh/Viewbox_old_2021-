namespace Viewbox
{
	public class BackStack
	{
		public string Link;

		public int Optimization;

		public BackStack(string Link, int Optimization)
		{
			this.Link = Link;
			this.Optimization = Optimization;
		}

		public BackStack Clone()
		{
			return new BackStack(Link, Optimization);
		}

		public override string ToString()
		{
			return "Link: " + Link + ", OptId: " + Optimization;
		}

		public override bool Equals(object obj)
		{
			return obj is BackStack && (obj as BackStack).Link == Link && (obj as BackStack).Optimization == Optimization;
		}
	}
}
