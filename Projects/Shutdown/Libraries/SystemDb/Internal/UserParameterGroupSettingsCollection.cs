using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SystemDb.Internal
{
	public class UserParameterGroupSettingsCollection : IUserParameterGroupSettingsCollection, IEnumerable<IUserParameterGroupSettings>, IEnumerable
	{
		private readonly Dictionary<int, UserParameterGroupSettings> _dic = new Dictionary<int, UserParameterGroupSettings>();

		public UserParameterGroupSettings this[int id]
		{
			get
			{
				if (!_dic.ContainsKey(id))
				{
					return null;
				}
				return _dic[id];
			}
		}

		public IEnumerable<IUserParameterGroupSettings> this[IUser user, IIssue issue] => _dic.Values.Where((UserParameterGroupSettings s) => s.IssueId == issue.Id && s.UserId == user.Id);

		public IEnumerator<IUserParameterGroupSettings> GetEnumerator()
		{
			return _dic.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Add(UserParameterGroupSettings settings)
		{
			_dic[settings.Id] = settings;
		}

		public void Remove(IUserTableColumnWidthsSettings settings)
		{
			_dic.Remove(settings.Id);
		}
	}
}
