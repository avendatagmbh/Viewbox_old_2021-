using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewBuilder.Models.MetadataUpdate
{
	public class ResolvingColumnsEventArgs : System.EventArgs
	{
		public bool AllColumnsWereResolved { get; private set; }
		public string TableName { get; private set; }

		public ResolvingColumnsEventArgs(bool allColumnsWereResolved, string tableName)
		{
			AllColumnsWereResolved = allColumnsWereResolved;
			TableName = tableName;
		}
	}
}
