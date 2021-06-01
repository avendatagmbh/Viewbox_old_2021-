using System;

namespace ViewboxMdConverter
{
	public class FileException : Exception
	{
		public FileException(string msg)
			: base(msg)
		{
		}
	}
}
