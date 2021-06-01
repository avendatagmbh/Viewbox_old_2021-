namespace Utils
{
	public class RomanNumeralPair
	{
		private readonly string _stringValue;

		private readonly int _value;

		public int Value => _value;

		public string StringValue => _stringValue;

		public RomanNumeralPair(int value, string stringValue)
		{
			_value = value;
			_stringValue = stringValue;
		}
	}
}
