using System;
using System.Collections.Generic;

namespace ProjectDb
{
    public class VersionInfo
    {
        private static VersionInfo _instance;

        private VersionInfo()
        {
        }

        public static VersionInfo Instance
        {
            get { return _instance ?? (_instance = new VersionInfo()); }
        }

        #region Properties

        public string CurrentDbVersion = "1.0.5";

        #region DBVersionHistory

        private readonly List<string> _dbVersionHistory = new List<string>
                                                              {
                                                                  "1.0.1",
                                                                  "1.0.2",
                                                                  "1.0.3",
                                                                  "1.0.4",
                                                                  "1.0.5"
                                                              };

        public List<string> DbVersionHistory
        {
            get { return _dbVersionHistory; }
        }

        #endregion

        #endregion

        //Returns a number to a version string of the form x.y.z, where higher revision have high numbers ("2.0.0" > "1.5.5")
        public static double VersionToDouble(string version)
        {
            string[] numbers = version.Split('.');
            double result = 0d;
            double pow = 1d;
            foreach (string number in numbers)
            {
                result += Convert.ToDouble(number)*pow;
                pow /= Convert.ToDouble(number.Length*10);
            }
            return result;
        }

        public bool NewerDbVersionExists(string version)
        {
            return VersionToDouble(DbVersionHistory[DbVersionHistory.Count - 1]) > VersionToDouble(version);
        }

        public string GetNextDbVersion(string version)
        {
            for (int i = 0; i < DbVersionHistory.Count; ++i)
                if (VersionToDouble(DbVersionHistory[i]) > VersionToDouble(version)) return DbVersionHistory[i];
            return "";
        }

        public string GetLastDbVersion(string version)
        {
            for (int i = DbVersionHistory.Count - 1; i >= 0; --i)
                if (VersionToDouble(DbVersionHistory[i]) <= VersionToDouble(version)) return DbVersionHistory[i];
            return "";
        }

        /// <summary>
        ///   Returns the previous database version as string
        /// </summary>
        /// <param name="version"> The base version </param>
        /// <returns> The version string of the pre-version </returns>
        public string GetPreviousDbVersion(string version)
        {
            for (int i = DbVersionHistory.Count - 1; i >= 0; --i)
                if (VersionToDouble(DbVersionHistory[i]) < VersionToDouble(version)) return DbVersionHistory[i];
            return string.Empty;
        }
    }
}