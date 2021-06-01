using System;
using System.IO;
using System.Text;

namespace Viewbox.Models
{
	public class LogReadModel : SettingsModel
	{
		public StringBuilder text = null;

		public override string Partial => "_LogReadPartial";

		public LogReadModel()
			: base(SettingsType.LogRead)
		{
			text = new StringBuilder();
			text.Clear();
			ReadFile();
		}

		public void ReadFile()
		{
			try
			{
				using StreamReader reader = new StreamReader(ViewboxApplication.LogPath);
				while (Add(reader.ReadLine()) != null)
				{
				}
			}
			catch (Exception)
			{
			}
		}

		public object Add(string sentence)
		{
			if (text != null)
			{
				text.AppendLine(sentence);
			}
			return sentence;
		}
	}
}
