using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class NotesCollection : Dictionary<IUser, List<INote>>, INotesCollection
	{
		public new List<INote> this[IUser user]
		{
			get
			{
				if (!ContainsKey(user))
				{
					return null;
				}
				return base[user];
			}
		}

		public void Add(INote note)
		{
			if (ContainsKey(note.User))
			{
				base[note.User].Add(note);
				return;
			}
			Add(note.User, new List<INote> { note });
		}
	}
}
