using System;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("notes", ForceInnoDb = true)]
	internal class Note : INote
	{
		private IUser _user;

		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		public IUser User
		{
			get
			{
				return _user;
			}
			set
			{
				if (_user != value)
				{
					_user = value as User;
					UserId = _user.Id;
				}
			}
		}

		[DbColumn("title")]
		public string Title { get; set; }

		[DbColumn("text", Length = 65536)]
		public string Text { get; set; }

		[DbColumn("date")]
		public DateTime Date { get; set; }
	}
}
