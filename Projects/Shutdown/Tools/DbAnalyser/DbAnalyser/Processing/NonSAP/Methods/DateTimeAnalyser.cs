using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DbAnalyser.Processing.NonSAP.Methods
{
    class DateTimeAnalyser : DateAnalyser
    {
        private List<string> hourFormats;
        private List<string> minuteAndSecodFormats;
        private List<string> timeZoneInfo;

        private static Regex lastUsedRegex;
        private static List<Regex> regexDateTimeCache;

        protected DateTimeAnalyser()
        {
            hourFormats = new List<string>();
            minuteAndSecodFormats = new List<string>();
            timeZoneInfo = new List<string>();
            regexDateTimeCache = new List<Regex>();

            hourFormats.Add("([0-2]{0,1}[0-9]{1})");
            minuteAndSecodFormats.Add("([0-5]{0,1}[0-9]{1})");

            timeZoneInfo.Add("((\\+|\\-)[0-9]{2}:[0-9]{2})");
            timeZoneInfo.Add(" (CET|CEDT)");

            initDateTypes();
            initDateWithTimes();
            expressions = generateDistinctRegexList();
        }

        private static DateTimeAnalyser dateTimeFilter;

        public static DateTimeAnalyser DateTimeFilter
        {
            get
            {
                if (dateTimeFilter == null)
                {
                    dateTimeFilter = new DateTimeAnalyser();
                }
                return dateTimeFilter;
            }
        }

        protected List<Regex> generateDistinctRegexList()
        {
            return expressions.Select(exp => exp.ToString()).Distinct().ToList().Select(item => new Regex(item)).ToList();
        }

        public int regexCount()
        {
            return expressions.Count;
        }
        protected override void initDateTypes()
        {
            foreach (string year in yearFormats)
            {
                foreach (string month in monthFormats)
                {
                    foreach (string day in daysFormats)
                    {
                        foreach (string separatorChar in separtorFormats)
                        {
                            string separator = "\\" + separatorChar;
                            dateFormats.Add("^" + year + separator + month + separator + day);
                            dateFormats.Add("^" + day + separator + month + separator + year);
                            dateFormats.Add("^" + month + separator + day + separator + year);

                            dateFormats.Add("^" + year + separator + monthWordFormat + separator + day);
                            dateFormats.Add("^" + day + separator + monthWordFormat + separator + year);
                            dateFormats.Add("^" + monthWordFormat + separator + day + separator + year);

                            dateFormats.Add("^" + year + separator + month + separator + day + "\\.");
                            dateFormats.Add("^" + day + separator + month + separator + year + "\\.");
                            dateFormats.Add("^" + month + separator + day + separator + year + "\\.");

                            dateFormats.Add("^" + year + separator + monthWordFormat + separator + day + "\\.");
                            dateFormats.Add("^" + day + separator + monthWordFormat + separator + year + "\\.");
                            dateFormats.Add("^" + monthWordFormat + separator + day + separator + year + "\\.");
                        }
                    }
                }
            }
        }

        protected void initDateWithTimes()
        {
            foreach (string hour in hourFormats)
            {
                foreach (string minute in minuteAndSecodFormats)
                {
                    foreach (string second in minuteAndSecodFormats)
                    {
                        timeFormats.Add(hour + ":" + minute + ":" + second + " ((a|p|A|P)(m|M))$");
                        timeFormats.Add(hour + "\\." + minute + "\\." + second + " ((a|p|A|P)(m|M))$");

                        timeFormats.Add(hour + ":" + minute + " ((a|p|A|P)(m|M))$");
                        timeFormats.Add(hour + "\\." + minute + " ((a|p|A|P)(m|M))$");

                        timeFormats.Add(hour + ":" + minute + ":" + second + "$");
                        timeFormats.Add(hour + "\\." + minute + "\\." + second + "$");

                        timeFormats.Add(hour + ":" + minute + "$");
                        timeFormats.Add(hour + "\\." + minute + "$");

                        foreach (var timeZone in timeZoneInfo)
                        {
                            timeFormats.Add(hour + ":" + minute + ":" + second + timeZone + "$");
                            timeFormats.Add(hour + "\\." + minute + "\\." + second + timeZone + "$");

                            timeFormats.Add(hour + ":" + minute + timeZone + "$");
                            timeFormats.Add(hour + "\\." + minute + timeZone + "$");
                        }
                    }
                }
            }
            foreach (var dateRegex in dateFormats)
            {
                foreach (var timeRegex in timeFormats)
                {
                    expressions.Add(new Regex(dateRegex + " " + timeRegex));
                }
            }
        }

        public override string AnalyzeInput(string input)
        {
            if (lastUsedRegex != null)
            {
                if (lastUsedRegex.IsMatch(input))
                {
                    return "DATETIME";
                }
            }
            if (regexDateTimeCache.Any(exp => exp.IsMatch(input)))
            {
                return "DATETIME";
            }
            foreach (var exp in expressions)
            {
                if (exp.IsMatch(input))
                {
                    lastUsedRegex = exp;
                    if (!regexDateTimeCache.Contains(exp))
                    {
                        regexDateTimeCache.Add(exp);
                    }
                    return "DATETIME";
                }
            }
            return "unknown";
        }

        public static List<Regex> getUsedRegexes()
        {
            return regexDateTimeCache;
        }

        public override string GetDateFormat(ref List<string> inputDate)
        {
            string format = base.GetDateFormat(ref inputDate);
            if (format == String.Empty)
            {
                return String.Empty;
            }
            return format + " %H:%i:%s";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                expressions = null;
                dateFormats = null;
                daysFormats = null;
                monthFormats = null;
                monthWordFormat = null;
                separtorFormats = null;
                timeFormats = null;
                yearFormats = null;

                minuteAndSecodFormats = null;
                hourFormats = null;
                timeFormats = null;
                timeZoneInfo = null;
            }

            disposed = true;
        }
    }
}
