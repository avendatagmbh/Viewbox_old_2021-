namespace ViewboxDb
{
	public class Sort
	{
		public int cid { get; set; }

		public SortDirection dir { get; set; }

		public Sort()
		{
		}

		public Sort(int cid, SortDirection dir)
		{
			this.cid = cid;
			this.dir = dir;
		}

		public override int GetHashCode()
		{
			string hashString = cid + dir.ToString();
			return 0 + hashString.GetHashCode();
		}

		public Sort Clone()
		{
			return new Sort(cid, dir);
		}
	}
}
