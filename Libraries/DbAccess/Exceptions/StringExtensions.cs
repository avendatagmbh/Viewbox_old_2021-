using System.Text.RegularExpressions;

namespace DbAccess.Exceptions
{
	public static class StringExtensions
	{
		public static string FromCamelCaseToLowerUnderscore(this string name)
		{
			return new Regex("([a-z])([A-Z])").Replace(name, "$1_$2").ToLower();
		}
	}
}
