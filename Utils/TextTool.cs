using System;

namespace Utils
{
	public static class TextTool
	{
		public static int CountStringOccurrences(string text, string pattern)
		{
			int count = 0;
			int i = 0;
			while ((i = text.IndexOf(pattern, i, StringComparison.InvariantCultureIgnoreCase)) != -1)
			{
				i += pattern.Length;
				count++;
			}
			return count;
		}
	}
}
