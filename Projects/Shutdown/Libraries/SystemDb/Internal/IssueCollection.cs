using System.Collections;
using System.Collections.Generic;

namespace SystemDb.Internal
{
	internal class IssueCollection : DataObjectCollection<Issue, IIssue>, IIssueCollection, IDataObjectCollection<IIssue>, IEnumerable<IIssue>, IEnumerable
	{
		public void AddToIssueCollection(IIssue issue)
		{
			Add(issue);
		}
	}
}
