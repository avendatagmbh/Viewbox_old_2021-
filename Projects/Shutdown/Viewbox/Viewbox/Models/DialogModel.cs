using System;
using System.Collections.Generic;

namespace Viewbox.Models
{
	public class DialogModel
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

			public string Url { get; set; }

			public string Id { get; set; }

			public Button()
			{
				Url = "#";
				Id = string.Empty;
			}
		}

		public string DialogTemplate { get; set; }

		public string Title { get; internal set; }

		public string Key { get; internal set; }

		public string Link { get; internal set; }

		public string Content { get; internal set; }

		public Type DialogType { get; internal set; }

		public List<Button> Buttons { get; internal set; }

		public string InputName { get; internal set; }

		public string InputValue { get; internal set; }

		public int InputLength { get; internal set; }

		public string InputType { get; internal set; }

		public string FormAction { get; internal set; }

		public string FormMethod { get; internal set; }

		public List<Tuple<string, string, string, string, int, int>> Inputs { get; internal set; }

		public Dictionary<string, string> InputMessages { get; internal set; }

		public List<Tuple<string, string, string>> HiddenFields { get; internal set; }

		public List<Tuple<string, string, List<string>, List<string>, int>> Select { get; internal set; }

		public string Class { get; internal set; }

		public List<Tuple<string, string, string>> Upload { get; internal set; }

		public DialogModel()
		{
			Buttons = new List<Button>();
			Inputs = new List<Tuple<string, string, string, string, int, int>>();
			InputMessages = new Dictionary<string, string>();
			HiddenFields = new List<Tuple<string, string, string>>();
		}
	}
}
