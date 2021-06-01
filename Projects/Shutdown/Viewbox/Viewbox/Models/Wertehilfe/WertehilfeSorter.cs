using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Viewbox.Models.Wertehilfe
{
	public class WertehilfeSorter
	{
		private List<WertehilfeSortObject> sortObjects = new List<WertehilfeSortObject>();

		public List<WertehilfeSortObject> Sorts => sortObjects;

		public WertehilfeSorter(string[] names, string[] directions)
		{
			if (names != null && directions != null)
			{
				for (int i = 0; i < names.Length; i++)
				{
					sortObjects.Add(new WertehilfeSortObject
					{
						Name = names[i],
						Direction = directions[i]
					});
				}
			}
		}

		public string GetSortingLogic(string column)
		{
			StringBuilder orderLogic = new StringBuilder();
			orderLogic.Append(" ORDER BY ");
			if (sortObjects.Count > 0)
			{
				foreach (WertehilfeSortObject sort in sortObjects)
				{
					orderLogic.Append($" {sort.Name} {sort.Direction},");
				}
				orderLogic.Length--;
			}
			else
			{
				orderLogic.Append($"{column} ASC");
			}
			return orderLogic.ToString();
		}

		public override string ToString()
		{
			return string.Join("**", sortObjects.Select((WertehilfeSortObject s) => s.Name + "__" + s.Direction));
		}
	}
}
