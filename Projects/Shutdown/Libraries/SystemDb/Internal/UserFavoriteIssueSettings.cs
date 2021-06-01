using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_favorite_issue_settings", ForceInnoDb = true)]
	internal class UserFavoriteIssueSettings : IUserFavoriteIssueSettings
	{
		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		public IUser User { get; set; }

		[DbColumn("favorites", Length = 10000)]
		public string FavoriteList { get; set; }

		public object Clone()
		{
			return new UserFavoriteIssueSettings
			{
				Id = Id,
				FavoriteList = FavoriteList,
				User = User,
				UserId = UserId
			};
		}
	}
}
