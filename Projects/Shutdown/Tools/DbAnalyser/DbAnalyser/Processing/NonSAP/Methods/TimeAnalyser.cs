using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DbAnalyser.Processing.NonSAP.Methods
{
    class TimeAnalyser : TypeAnalyser
    {
        private List<string> hourFormats;
        private List<string> minuteAndSecodFormats;
        private List<string> timeZoneInfo;
        private List<string> timeFormats;

        private static Regex lastUsedRegex;
        private static List<Regex> regexTimeCache;

        protected TimeAnalyser()
        {
            hourFormats = new List<string>();
            minuteAndSecodFormats = new List<string>();
            timeZoneInfo = new List<string>();
            timeFormats = new List<string>();
            regexTimeCache = new List<Regex>();

            hourFormats.Add("([0-2]{0,1}[0-9]{1})");
            minuteAndSecodFormats.Add("([0-5]{0,1}[0-9]{1})");

            timeZoneInfo.Add("((\\+|\\-)[0-9]{2}:[0-9]{2})");
            timeZoneInfo.Add(" (CET|CEDT)");

            InitTimeAnalyser();
        }

        private static TimeAnalyser timeFilter;

        public static TimeAnalyser TimeFilter
        {
            get
            {
                if (timeFilter == null)
                {
                    timeFilter = new TimeAnalyser();
                }
                return timeFilter;
            }
        }
        
        private void InitTimeAnalyser()
        {
            foreach (string hour in hourFormats)
            {
                foreach (string minute in minuteAndSecodFormats)
                {
                    foreach (string second in minuteAndSecodFormats)
                    {
                        timeFormats.Add("^" + hour + ":" + minute + ":" + second + " ((a|p|A|P)(m|M))$");

                        timeFormats.Add("^" + hour + ":" + minute + " ((a|p|A|P)(m|M))$");

                        timeFormats.Add("^" + hour + ":" + minute + ":" + second + "$");

                        timeFormats.Add("^" + hour + ":" + minute + "$");

                        foreach (var timeZone in timeZoneInfo)
                        {
                            timeFormats.Add("^" + hour + ":" + minute + ":" + second + timeZone + "$");

                            timeFormats.Add("^" + hour + ":" + minute + timeZone + "$");
                        }
                    }
                }
            }

            foreach (var timeRegex in timeFormats)
            {
                expressions.Add(new Regex(timeRegex));
            }
        }

        public override string AnalyzeInput(string input)
        {
            if (lastUsedRegex != null)
            {
                if (lastUsedRegex.IsMatch(input))
                {
                    return "TIME";
                }
            }
            foreach (var exp in regexTimeCache)
            {
                if (exp.IsMatch(input))
                {
                    return "TIME";
                }
            }
            foreach (var exp in expressions)
            {
                if (exp.IsMatch(input))
                {
                    lastUsedRegex = exp;
                    if (!regexTimeCache.Contains(exp))
                    {
                        regexTimeCache.Add(exp);
                    }
                    return "TIME";
                }
            }
            return "unknown";
        }

        public static List<Regex> GetUsedRegexes()
        {
            return regexTimeCache;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                expressions = null;
                timeFormats = null;

                minuteAndSecodFormats = null;
                hourFormats = null;
                timeFormats = null;
                timeZoneInfo = null;
            }

            disposed = true;
        }
    }
}
