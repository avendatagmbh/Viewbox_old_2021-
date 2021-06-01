using System;

namespace Utils
{
	public class ErrorEventArgs : EventArgs
	{
		public string Message { get; set; }

		public ErrorEventArgs(string message)
		{
			Message = message;
		}
	}
}
