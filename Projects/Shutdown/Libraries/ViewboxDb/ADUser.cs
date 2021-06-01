namespace ViewboxDb
{
	public class ADUser
	{
		public string Domain { get; set; }

		public string ShortName { get; set; }

		public string Name { get; set; }

		public ADUser()
		{
		}

		public ADUser(string domain, string shortName, string name)
		{
			Domain = domain;
			ShortName = shortName;
			Name = name;
		}
	}
}
