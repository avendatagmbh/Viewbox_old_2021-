using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Utils
{
	public class RomanNumeralizer : IValueConverter
	{
		public static RomanNumeralizer RomanNumeralizerInstance = new RomanNumeralizer();

		private IEnumerable<RomanNumeralPair> PairSet => new List<RomanNumeralPair>
		{
			new RomanNumeralPair(1000, "M"),
			new RomanNumeralPair(900, "CM"),
			new RomanNumeralPair(500, "D"),
			new RomanNumeralPair(400, "CD"),
			new RomanNumeralPair(100, "C"),
			new RomanNumeralPair(90, "XC"),
			new RomanNumeralPair(50, "L"),
			new RomanNumeralPair(40, "XL"),
			new RomanNumeralPair(10, "X"),
			new RomanNumeralPair(9, "IX"),
			new RomanNumeralPair(5, "V"),
			new RomanNumeralPair(4, "IV"),
			new RomanNumeralPair(1, "I")
		};

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return ConvertToRomanNumeral(System.Convert.ToInt32(value));
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}

		public string Convert(int value)
		{
			object obj = Convert(value, null, null, CultureInfo.CurrentCulture);
			if (obj != null)
			{
				return obj.ToString();
			}
			return "";
		}

		private string ConvertToRomanNumeral(int input)
		{
			StringBuilder myBuilder = new StringBuilder();
			foreach (RomanNumeralPair thisPair in PairSet)
			{
				while (input >= thisPair.Value)
				{
					myBuilder.Append(thisPair.StringValue);
					input -= thisPair.Value;
				}
			}
			return myBuilder.ToString();
		}
	}
}
