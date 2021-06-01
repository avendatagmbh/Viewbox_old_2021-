using System;
using System.Collections.Generic;

namespace Viewbox.Models
{
	public class DocumentsOverviewModel : ViewboxModel
	{
		public string Body { get; set; }

		public bool IsHtml { get; set; }

		public string Name { get; set; }

		public string Extension { get; set; }

		public int TableId { get; set; }

		public long TicketNumber { get; internal set; }

		public int TicketNumberColumnId { get; internal set; }

		public long OnItemNumber { get; internal set; }

		public int OnItemNumberColumnId { get; internal set; }

		public long BackToItemNumber { get; internal set; }

		public long NextToItemNumber { get; internal set; }

		public long ItemNumberCount { get; internal set; }

		public List<Tuple<string, string>> Attachments { get; internal set; }

		public override string LabelCaption
		{
			get
			{
				throw new NotImplementedException();
			}
		}
	}
}
