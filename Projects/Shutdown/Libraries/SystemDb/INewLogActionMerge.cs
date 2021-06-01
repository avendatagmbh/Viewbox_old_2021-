using System;

namespace SystemDb
{
	public interface INewLogActionMerge
	{
		int Id { get; set; }

		int UserId { get; set; }

		DateTime Timestamp { get; set; }

		int ActionController { get; set; }

		string QueryString { get; set; }

		int Parentid { get; set; }
	}
}
