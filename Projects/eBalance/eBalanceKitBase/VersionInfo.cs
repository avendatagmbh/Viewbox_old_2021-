//#define old
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBase {
    public class VersionInfo {
        private VersionInfo(){}
        private static VersionInfo _instance;

        public static VersionInfo Instance {
            get { return _instance ?? (_instance = new VersionInfo()); }
        }

        #region Properties
        
        #region CurrentVersion
#if old
        private string currentVersion = "1.5.90";
#else
        private string currentVersion = "1.6";
#endif
        public string CurrentVersion {
            get { return currentVersion; }
            private set { currentVersion = value; }
        }
        #endregion

        #region CurrentDbVersion

#if old
        private string currentDbVersion = "1.5.0";//"1.5.9";
#else
        private string currentDbVersion = "1.6.0";
#endif

        public string CurrentDbVersion {
            get { return currentDbVersion; }
            private set { currentDbVersion = value; }
        }
        #endregion
        
        #region DBVersionHistory
        
        private List<string> _dbVersionHistory = new List<string>(){
            "1.0.0",
            "1.1.6",
            "1.1.8",
            "1.1.9",
            "1.2.0",
            "1.2.1",
            "1.3.0",
            "1.3.1",
            "1.4.0",
            "1.5.0"
#if !old
            , "1.5.9"
            ,"1.6.0"
#endif
        };

        public List<string> DbVersionHistory {
            get { return _dbVersionHistory; }
            private set { _dbVersionHistory = value; }
        }
        #endregion

        #endregion

        #region VersionToDouble
        //Returns a number to a version string of the form x.y.z, where higher revision have high numbers ("2.0.0" > "1.5.5")
        public double VersionToDouble(string version) {
            if (string.IsNullOrEmpty(version)) return VersionToDouble(CurrentDbVersion);
            string[] numbers = version.Split('.');
            double result = 0d;
            double pow = 1d;
            
            //int pow=10*10*10*10*10;
            //for (int i = 0; i < numbers.Length; ++i) {
            //    result += Convert.ToInt32(numbers[i]) * pow;
            //    pow /= 10;
            //}
            foreach (string number in numbers) {
                result += Convert.ToDouble(number) * pow;
                pow /= Convert.ToDouble(number.Length * 10);
            }
            return result;
        }
        #endregion

        public bool ProgramVersionToOld(string dbVersion) {
            return VersionToDouble(dbVersion) > VersionToDouble(CurrentVersion);
        }

        public bool NewerDbVersionExists(string version) {
            return VersionToDouble(DbVersionHistory[DbVersionHistory.Count - 1]) > VersionToDouble(version);
        }

        public string GetNextDbVersion(string version) {
            for (int i = 0; i < DbVersionHistory.Count; ++i)
                if (VersionToDouble(DbVersionHistory[i]) > VersionToDouble(version)) return DbVersionHistory[i];
            return "";
        }

        public string GetLastDbVersion(string version) {
            for (int i = DbVersionHistory.Count-1; i >= 0; --i)
                if (VersionToDouble(DbVersionHistory[i]) <= VersionToDouble(version)) return DbVersionHistory[i];
            return "";
        }

        /// <summary>
        /// Returns the previous database version as string
        /// </summary>
        /// <param name="version">The base version</param>
        /// <returns>The version string of the pre-version</returns>
        public string GetPreviousDbVersion(string version) {
            for (int i = DbVersionHistory.Count-1; i >= 0; --i)
                if (VersionToDouble(DbVersionHistory[i]) < VersionToDouble(version)) return DbVersionHistory[i];
            return string.Empty;
        }

    }
}
