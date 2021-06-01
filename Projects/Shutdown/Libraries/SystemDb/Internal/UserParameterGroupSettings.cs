using SystemDb.Helper;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("user_parameter_groups")]
	public class UserParameterGroupSettings : IUserParameterGroupSettings
	{
		private readonly LocalizedTextCollection _descriptions = new LocalizedTextCollection();

		[DbColumn("user_id")]
		public int UserId { get; set; }

		[DbColumn("issue_id")]
		public int IssueId { get; set; }

		[DbColumn("id")]
		[DbPrimaryKey]
		public int Id { get; set; }

		public IUser User { get; set; }

		public IIssue Issue { get; set; }

		[DbColumn("parameter_group", Length = 100000)]
		public string ParameterGroup { get; set; }

		public ILocalizedTextCollection Descriptions => _descriptions;

		public void SetDescription(string description, ILanguage language)
		{
			_descriptions.Add(language, description);
		}
	}
}
