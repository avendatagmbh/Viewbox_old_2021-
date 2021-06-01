using System.Collections;
using System.Collections.Generic;

namespace SystemDb
{
	public interface IIssueCollection : IDataObjectCollection<IIssue>, IEnumerable<IIssue>, IEnumerable
	{
		void AddToIssueCollection(IIssue issue);
	}
}
