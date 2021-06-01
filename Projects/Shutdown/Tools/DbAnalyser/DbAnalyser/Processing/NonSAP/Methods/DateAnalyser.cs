using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DbAnalyser.Processing.NonSAP.Methods
{
    class DateAnalyser : TypeAnalyser
    {
        protected List<string> timeFormats;
        protected List<string> dateFormats;
        protected List<string> yearFormats;
        protected List<string> monthFormats;
        protected string monthWordFormat;
        protected List<string> daysFormats;
        protected List<string> separtorFormats;


        private static Regex lastUsedRegex; // last user regular expression
        private static List<Regex> regexDateCache; // regular expression, which were used in the process

        protected DateAnalyser() : base() 
        {
            timeFormats     = new List<string>();
            dateFormats     = new List<string>();
            yearFormats     = new List<string>();
            monthFormats    = new List<string>();
            daysFormats    = new List<string>();
            separtorFormats = new List<string>();
            regexDateCache  = new List<Regex>();

            yearFormats.Add("((1|2)[0-9]{3})"); // 15
            yearFormats.Add("([0-9]{2})"); // 10
            monthFormats.Add("(0[0-9]{1}|1[0-2]{1})"); // 21
            monthWordFormat = "((January|Januar|Jan[.]?|JAN[.]?" +
                                   "|February|Februar|Feb[.]?|FEB[.]?" +
                                   "|March|März|Mar[.]?|Mär[.]?|MAR[.]?|MÄR[.]?" +
                                   "|April|Apr[.]?|APR[.]?" +
                                   "|Mai[.]?|May[.]?|MAI[.]?|MAY[.]?" +
                                   "|June|Juni|Jun[.]?|JUN[.]?" +
                                   "|July|Juli|Jul[.]?|JUL[.]?" +
                                   "|August|Aug[.]?|AUG[.]?" +
                                   "|September|Sep[.]?|SEP[.]?" +
                                   "|October|Oktober|Oct[.]?|Okt[.]?|OCT[.]?|OKT[.]?" +
                                   "|November|Nov[.]?|NOV[.]?" +
                                   "|December|Dezember|Dec[.]?|Dez[.]?|DEC[.]?|DEZ[.]?))";
            daysFormats.Add("((0|1|2)[0-9]|3[0-1]{1})"); // 24

            separtorFormats.Add(".");
            separtorFormats.Add("/");
            separtorFormats.Add("-");

            initDateTypes();
        }

        private static DateAnalyser dateFilter;

        public static DateAnalyser DateFilter
        {
            get
            {
                if (dateFilter == null)
                {
                    dateFilter = new DateAnalyser();
                }
                return dateFilter;
            }
        }

        protected virtual void initDateTypes()
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

                            //dateFormats.Add("^" + year + separator + monthWordFormat + separator + day);
                            //dateFormats.Add("^" + day + separator + monthWordFormat + separator + year);
                            //dateFormats.Add("^" + monthWordFormat + separator + day + separator + year);

                            dateFormats.Add("^" + year + separator + month + separator + day + "\\.");
                            dateFormats.Add("^" + day + separator + month + separator + year + "\\.");
                            dateFormats.Add("^" + month + separator + day + separator + year + "\\.");

                            //dateFormats.Add("^" + year + separator + monthWordFormat + separator + day + "\\.");
                            //dateFormats.Add("^" + day + separator + monthWordFormat + separator + year + "\\.");
                            //dateFormats.Add("^" + monthWordFormat + separator + day + separator + year + "\\.");
                        }
                    }    
                }
            }

            foreach (string regex in dateFormats)
            {
                expressions.Add(new Regex(regex + "$"));
            }
        }

        public override string AnalyzeInput(string input)
        {
            // testing with the regex used last time
            if (lastUsedRegex != null)
            {
                if (lastUsedRegex.IsMatch(input))
                {
                    return "DATE";
                }
            }
            // testing with the recently used regexes
            foreach (var exp in regexDateCache)
            {
                if (exp.IsMatch(input))
                {
                    return "DATE";
                }    
            }
            // testing with every possible regex
            foreach (var exp in expressions)
            {
                if (exp.IsMatch(input))
                {
                    lastUsedRegex = exp;
                    if (!regexDateCache.Contains(exp))
                    {
                        regexDateCache.Add(exp);
                    }
                    return "DATE";
                }
            }
            return "unknown";
        }

        public static List<Regex> getUsedRegexes()
        {
            return regexDateCache;
        }

        public virtual string GetDateFormat(ref List<string> inputDates)
        {
            string usedSeparator = "";            

            for (int i = 0; i < separtorFormats.Count; i++)
            {
                foreach (string inputDate in inputDates)
                {
                    string date;
                    if (inputDate.Count() > 11)
                    {
                        string[] parts = inputDate.Split(' ');
                        date = parts[0];
                    }
                    else
                    {
                        date = inputDate;
                    }

                    string[] dateParts = date.Split(separtorFormats[i].ToCharArray());
                    if (dateParts[0].Count() == 2 || dateParts[0].Count() == 4)
	                {
                        try
                        {
                            usedSeparator = separtorFormats[i];
                            int first = Int32.Parse(dateParts[0]);
                            int second = Int32.Parse(dateParts[1]);
                            int third = Int32.Parse(dateParts[2]);

                            if (first > 100)
                            {
                                if (second > 12)
                                {
                                    return "%Y" + separtorFormats[i] + "%d" + separtorFormats[i] + "%m"; // 2005.24.12
                                }
                                if (third > 12)
                                {
                                    return "%Y" + separtorFormats[i] + "%m" + separtorFormats[i] + "%d"; // 2001.12.24
                                }
                                return "%Y" + separtorFormats[i] + "%m" + separtorFormats[i] + "%d"; // 2001.12.24
                            }
                            if (second > 100)
                            {
                                if (first > 12)
                                {
                                    return "%d" + separtorFormats[i] + "%Y" + separtorFormats[i] + "%m"; // 24.2001.12
                                }
                                if (third > 12)
                                {
                                    return "%m" + separtorFormats[i] + "%Y" + separtorFormats[i] + "%d"; // 12.2001.24
                                }
                            }
                            else if (third > 100)
                            {
                                if (first > 12)
                                {
                                    return "%d" + separtorFormats[i] + "%m" + separtorFormats[i] + "%Y"; // 24.12.2001
                                }
                                if (second > 12)
                                {
                                    return "%m" + separtorFormats[i] + "%d" + separtorFormats[i] + "%Y"; // 12.24.2001
                                }
                                return "%d" + separtorFormats[i] + "%m" + separtorFormats[i] + "%Y"; // 24.12.2001 
                            }
                        }
                        catch (Exception)
                        {
                            return String.Empty;
                        }
                        
	                }
                }
            } return "%Y" + usedSeparator + "%m" + usedSeparator + "%d";
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
            }

            disposed = true;
        }
    }
}
