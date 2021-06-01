namespace ViewboxDb
{
	public class JoinColumns
	{
		public int Column1 { get; set; }

		public int Column2 { get; set; }

		public SortDirection Direction { get; set; }

		public JoinColumns()
		{
		}

		public JoinColumns(int id, int joinTableId, int column1, int column2, SortDirection dir)
		{
			Column1 = column1;
			Column2 = column2;
			Direction = dir;
		}

		public override int GetHashCode()
		{
			string hashString = Column1 + Column2 + Direction.ToString();
			return 0 + hashString.GetHashCode();
		}
	}
}
