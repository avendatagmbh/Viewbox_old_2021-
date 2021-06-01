using System;
using System.Collections.Generic;
using Viewbox.LotusNotes;

namespace Viewbox.Models
{
	public class LotusNodeCollection : ViewboxModel
	{
		public List<LotusNode> Nodes { get; set; }

		public override string LabelCaption
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public LotusNodeCollection()
		{
			Nodes = new List<LotusNode>();
		}
	}
}
