using System.Collections.Generic;
using System.Globalization;

namespace PdfGenerator
{
	public class Headline
	{
		private Headline Parent { get; set; }

		public string Caption { private get; set; }

		private List<Headline> Children { get; set; }

		private int Level { get; set; }

		public int Ordinal { private get; set; }

		public string OrdinalNumber
		{
			get
			{
				string result = Ordinal.ToString(CultureInfo.InvariantCulture);
				if (Parent != null)
				{
					result = Parent.OrdinalNumber + "." + result;
				}
				return result;
			}
		}

		public string DisplayString => Caption;

		public Headline()
		{
			Children = new List<Headline>();
			Level = 1;
		}

		public Headline AddChildren(string caption)
		{
			Headline headline = new Headline
			{
				Caption = caption,
				Level = Level + 1,
				Parent = this,
				Ordinal = Children.Count + 1
			};
			Children.Add(headline);
			return headline;
		}
	}
}
