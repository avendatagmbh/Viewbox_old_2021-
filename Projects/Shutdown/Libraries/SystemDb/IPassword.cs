using System;

namespace SystemDb
{
	public interface IPassword : ICloneable
	{
		int Id { get; set; }

		string PasswordHash { get; set; }

		int UserId { get; set; }

		string PasswordText { get; set; }

		DateTime CreationDate { get; set; }
	}
}
