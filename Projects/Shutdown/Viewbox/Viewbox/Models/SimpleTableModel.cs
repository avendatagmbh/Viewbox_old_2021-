using System;
using System.Collections.Generic;
using System.Linq;
using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class SimpleTableModel : ViewboxModel
	{
		private List<Tuple<string, string>> _displayParameters = new List<Tuple<string, string>>();

		public List<Tuple<IParameter, string, string>> Parameters { get; private set; }

		public List<Tuple<string, string>> DisplayParameters => _displayParameters;

		public override string LabelCaption => "SimpleTableModel";

		public SimpleTableModel()
		{
			Parameters = new List<Tuple<IParameter, string, string>>();
		}

		public List<Tuple<string, string>> GetParameters(int tableId)
		{
			List<Tuple<string, string>> res = DisplayParameters;
			int num;
			if (res.Where((Tuple<string, string> r) => r.Item1 == Resources.CreationDate).Count() == 0)
			{
				_ = ViewboxSession.OpenIssueDate.FirstOrDefault((KeyValuePair<int, string> x) => x.Key == tableId).Key;
				num = ((ViewboxSession.OpenIssueDate.FirstOrDefault((KeyValuePair<int, string> x) => x.Key == tableId).Value != string.Empty) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			if (num != 0)
			{
				res.Add(new Tuple<string, string>(Resources.CreationDate, ViewboxSession.OpenIssueDate.FirstOrDefault((KeyValuePair<int, string> x) => x.Key == tableId).Value));
			}
			return res;
		}
	}
}
