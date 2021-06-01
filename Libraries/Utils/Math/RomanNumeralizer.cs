using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace ValueConverters
{
    public class RomanNumeralizer : IValueConverter
    {
        public static RomanNumeralizer RomanNumeralizerInstance = new RomanNumeralizer();

        private List<RomanNumeralPair> PairSet
        {
            get
            {
                List<RomanNumeralPair> output = new List<RomanNumeralPair>();
                output.Add(new RomanNumeralPair(1000, "M"));
                output.Add(new RomanNumeralPair(900, "CM"));
                output.Add(new RomanNumeralPair(500, "D"));
                output.Add(new RomanNumeralPair(400, "CD"));
                output.Add(new RomanNumeralPair(100, "C"));
                output.Add(new RomanNumeralPair(90, "XC"));
                output.Add(new RomanNumeralPair(50, "L"));
                output.Add(new RomanNumeralPair(40, "XL"));
                output.Add(new RomanNumeralPair(10, "X"));
                output.Add(new RomanNumeralPair(9, "IX"));
                output.Add(new RomanNumeralPair(5, "V"));
                output.Add(new RomanNumeralPair(4, "IV"));
                output.Add(new RomanNumeralPair(1, "I"));
                return output;
            }
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ConvertToRomanNumeral(System.Convert.ToInt32(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }

        #endregion

        public string Convert(int value)
        {
            object obj = Convert(value, null, null, CultureInfo.CurrentCulture);
            return obj == null ? "" : obj.ToString();
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

    public class RomanNumeralPair
    {
        private readonly string stringValue;
        private readonly int value;

        public RomanNumeralPair(int value, string stringValue)
        {
            this.value = value;
            this.stringValue = stringValue;
        }

        public int Value
        {
            get { return value; }
        }

        public string StringValue
        {
            get { return stringValue; }
        }
    }
}