using System;

namespace SystemDb
{
	public interface INote
	{
		int Id { get; }

		IUser User { get; }

		string Title { get; }

		string Text { get; }

		DateTime Date { get; }
	}
}
