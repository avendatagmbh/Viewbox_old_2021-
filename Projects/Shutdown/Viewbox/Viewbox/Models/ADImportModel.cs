using System;
using System.Collections.Generic;

namespace Viewbox.Models
{
	public class ADImportModel : BaseModel
	{
		public enum Type
		{
			Warning,
			Info,
			Mail
		}

		public class Button
		{
			public string Caption { get; internal set; }

			public string Data { get; internal set; }
		}

		public string Title { get; internal set; }

		public string Content { get; internal set; }

		public Type DialogType { get; internal set; }

		public List<Button> Buttons { get; internal set; }

		public List<Tuple<string, string, string, string, int>> Inputs { get; internal set; }

		public List<Tuple<string, string, string>> HiddenFields { get; internal set; }

		public Tuple<string, string, List<string>, List<string>, int> Select { get; internal set; }

		public string Class { get; internal set; }

		public List<Tuple<string, string, string>> Users { get; internal set; }

		public ADImportModel()
		{
			Buttons = new List<Button>();
			Inputs = new List<Tuple<string, string, string, string, int>>();
			HiddenFields = new List<Tuple<string, string, string>>();
		}
	}
}
