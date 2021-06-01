using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewBuilder.Models.MetadataUpdate
{
	class ResolvingTablesEventArgs : System.EventArgs
	{
		public int NumberOfTables { get; set; }

		public ResolvingTablesEventArgs(int numberOfTables)
		{
			NumberOfTables = numberOfTables;
		}
	}
}
