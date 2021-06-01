using System;
using System.Globalization;

namespace DbSearchLogic.SearchCore.SearchMatrix {
    /// <summary>
    /// Static parser class to convert search values into objects of other types
    /// </summary>
    public static class SearchValueParser {
        /// <summary>
        /// Gets a decimal or null value for the given string
        /// </summary>
        /// <param name="sValue">Value as string</param>
        /// <returns>Value as decimal or null</returns>
        public static decimal? GetDecimal(string sValue) {
            // Result flag
            bool bSuccess = false;
            // The decimal value
            decimal dValue;

            // Check if the value can be parsed to double
            if (decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-US"), out dValue) ||
                decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("de-DE"), out dValue)) {
                // Check if the value is just US or DE
                if (decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-US"), out dValue) &&
                    !decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("de-DE"), out dValue)) {
                    bSuccess = decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-US"), out dValue);
                } else if (!decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-US"), out dValue) &&
                         decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("de-DE"), out dValue)) {
                    bSuccess = decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("de-DE"), out dValue);
                }
                    // The value can be both (US and DE)
                else {
                    // Check if the value has points or commas
                    if (!sValue.Contains(".") && !sValue.Contains(",")) {
                        bSuccess = decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("de-DE"), out dValue);
                    }
                        // Check if "." is on a higher position than ","
                    else if (sValue.LastIndexOf(".") > sValue.LastIndexOf(",")) {
                        bSuccess = decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("en-US"), out dValue);
                    } else {
                        bSuccess = decimal.TryParse(sValue, NumberStyles.Any, CultureInfo.CreateSpecificCulture("de-DE"), out dValue);
                    }
                }
            }

            // If the value can't be parsed, return null...
            if (!bSuccess) {
                return null;
            }

            // ... else return the value
            return dValue;
        }

        /// <summary>
        /// Gets a decimal or null value for the given string (with a primary interpretation language)
        /// </summary>
        /// <param name="sValue">Value as string</param>
        /// <param name="oLanguage">Primary CultureInfo</param>
        /// <returns>Value as decimal or null</returns>
        public static decimal? GetDecimal(string sValue, CultureInfo oLanguage) {
            // The decimal value
            decimal dValue;

            // Define primary and secondary language
            CultureInfo oPrimary, oSecondary;
            if (oLanguage.Equals(Global.English)) {
                oPrimary = Global.English;
                oSecondary = Global.German;
            } else {
                oPrimary = Global.German;
                oSecondary = Global.English;
            }

            // Check if the value can be parsed to decimal
            if (decimal.TryParse(sValue, NumberStyles.Any, oPrimary, out dValue)) {
                return dValue;
            } else if (decimal.TryParse(sValue, NumberStyles.Any, oSecondary, out dValue)) {
                return dValue;
            } else {
                return new decimal?();
            }
        }


        /// <summary>
        /// Gets a DateTime or null value for the given string
        /// </summary>
        /// <param name="sValue">Value as string</param>
        /// <returns>Value as DateTime or null</returns>
        public static DateTime? GetDateTime(string sValue) {
            // Result flag
            bool bSuccess = false;
            // The DateTime value
            DateTime dValue;

            if (!DateTime.TryParse(sValue, CultureInfo.CreateSpecificCulture("de-DE"),
                                   DateTimeStyles.None, out dValue)) {
                bSuccess = DateTime.TryParse(sValue, CultureInfo.CreateSpecificCulture("en-US"),
                                             DateTimeStyles.None, out dValue);
            }

            // If the value can't be parsed, return null...
            if (!bSuccess) {
                return null;
            }

            // ... else return the value
            return dValue;
        }

        /// <summary>
        /// Gets a DateTime or null value for the given string (with a primary intepretation language)
        /// </summary>
        /// <param name="sValue">Value as string</param>
        /// <param name="oLanguage">Primary CultureInfo</param>
        /// <returns>Value as DateTime or null</returns>
        public static DateTime? GetDateTime(string sValue, CultureInfo oLanguage) {
            // The decimal value
            DateTime dValue;

            // Define primary and secondary language
            CultureInfo oPrimary, oSecondary;
            if (oLanguage.Equals(Global.English)) {
                oPrimary = Global.English;
                oSecondary = Global.German;
            } else {
                oPrimary = Global.German;
                oSecondary = Global.English;
            }

            // Check if the value can be parsed to DateTime
            if (DateTime.TryParse(sValue, oPrimary, DateTimeStyles.None, out dValue)) {
                return dValue;
            } else if (DateTime.TryParse(sValue, oSecondary, DateTimeStyles.None, out dValue)) {
                return dValue;
            } else {
                return new DateTime?();
            }
        }

        /// <summary>
        /// Gets a Integer or null value for the given string
        /// </summary>
        public static Int64? GetInteger(string value) {
            Int64 nValue;
            if (!Int64.TryParse(value, out nValue)) {
                return null;
            } else {
                return nValue;
            }
        }

        /// <summary>
        /// Gets a Integer or null value for the given string
        /// </summary>
        public static UInt64? GetUnsignedInteger(string value) {
            UInt64 nValue;
            if (!UInt64.TryParse(value, out nValue)) {
                return null;
            } else {
                return nValue;
            }
        }
    }
}