using System;

namespace Viewbox
{
	public class UrlParam
	{
		public string Name { get; private set; }

		public object Value { get; private set; }

		public UrlParam(string name, object value)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("name must not be null or whitespace", "name");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			Name = name.Trim();
			Value = value;
		}
	}
}
