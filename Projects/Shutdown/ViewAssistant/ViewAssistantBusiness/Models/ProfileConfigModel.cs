using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using DbAccess.Structures;
using ViewAssistantBusiness.Config;
using Utils;

namespace ViewAssistantBusiness
{
    public class ProfileConfigModel : NotifyPropertyChangedBase
    {
        #region Default properties

        public long Id { get; set; }

        #region Name
        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }
        #endregion Name

        #region DefaultMandtCol
        private string _defaultMandtCol;
        public string DefaultMandtCol
        {
            get
            {
                return _defaultMandtCol;
            }
            set
            {
                _defaultMandtCol = value;
                OnPropertyChanged("DefaultMandtCol");
            }
        }
        #endregion DefaultMandtCol

        #region DefaultBukrsCol
        private string _defaultBukrsCol;
        public string DefaultBukrsCol
        {
            get
            {
                return _defaultBukrsCol;
            }
            set
            {
                _defaultBukrsCol = value;
                OnPropertyChanged("DefaultBukrsCol");
            }
        }
        #endregion DefaultBukrsCol

        #region DefaultGjahrCol
        private string _defaultGjahrCol;
        public string DefaultGjahrCol
        {
            get
            {
                return _defaultGjahrCol;
            }
            set
            {
                _defaultGjahrCol = value;
                OnPropertyChanged("DefaultGjahrCol");
            }
        }
        #endregion DefaultGjahrCol

        #region ThreadsNumber
        private int _threadsNumber;
        public int ThreadsNumber
        {
            get
            {
                return _threadsNumber;
            }
            set
            {
                _threadsNumber = value;
                OnPropertyChanged("ThreadsNumber");
            }
        }

        private static readonly int[] _possibleNumberThreads = new[] {2, 3, 4, 5, 6};
        public static int[] PossibleNumberThreads
        {
            get { return _possibleNumberThreads; }
        }
        #endregion ThreadsNumber

        #region ShowRowCounts

        private bool _showRowCounts;
        public bool HideRowCounts
        {
            get { return _showRowCounts; }

            set
            {
                if (_showRowCounts != value)
                {
                    _showRowCounts = value;
                    OnPropertyChanged("HideRowCounts");
                }
            }
        }

        #endregion

        public List<string> DefaultMandtCols { get { return (DefaultMandtCol ?? "").ToLower().Split(';').Where(w=>!String.IsNullOrEmpty(w)).ToList(); } }
        public List<string> DefaultBukrsCols { get { return (DefaultBukrsCol ?? "").ToLower().Split(';').Where(w => !String.IsNullOrEmpty(w)).ToList(); } }
        public List<string> DefaultGjahrCols { get { return (DefaultGjahrCol ?? "").ToLower().Split(';').Where(w => !String.IsNullOrEmpty(w)).ToList(); } }
        
        public DbConfig SourceConnection { get; set; }
        public DbConfig ViewboxConnection { get; set; }
        public DbConfig FinalConnection { get; set; }

        #endregion

        public ProfileConfigModel()
        {
            DefaultMandtCol = "MANDT;";
            DefaultBukrsCol = "BUKRS;";
            DefaultGjahrCol = "GJAHR;";
            ThreadsNumber = 4;
            SourceConnection = new DbConfig() { DbType = "Access", Hostname = "", Password = "" };
            ViewboxConnection = new DbConfig() { DbType = "MySQL", Hostname = "localhost", Port = 3306, Username = "root", DbName = "", Password = "" };
            FinalConnection = new DbConfig() { DbType = "MySQL", Hostname = "localhost", Port = 3306, Username = "root", DbName = "", Password = "" };
        }

        public ProfileConfigModel(ProfileConfig profileConfig)
        {
            Id = profileConfig.Id;
            Name = profileConfig.Name;
            DefaultMandtCol = profileConfig.DefaultMandtCol;
            DefaultBukrsCol = profileConfig.DefaultBukrsCol;
            DefaultGjahrCol = profileConfig.DefaultGjahrCol;
            ThreadsNumber = profileConfig.ThreadsNumber;
            SourceConnection = new DbConfig() { DbType = profileConfig.SourceDbType, Hostname = profileConfig.SourceHost, Port = profileConfig.SourcePort, Username = profileConfig.SourceUser, DbName = profileConfig.SourceDbName, Password = profileConfig.SourcePassword };
            ViewboxConnection = new DbConfig() { DbType = profileConfig.ViewboxDbType, Hostname = profileConfig.ViewboxHost, Port = profileConfig.ViewboxPort, Username = profileConfig.ViewboxUser, DbName = profileConfig.ViewboxDbName, Password = profileConfig.ViewboxPassword };
            FinalConnection = new DbConfig() { DbType = profileConfig.FinalDbType, Hostname = profileConfig.FinalHost, Port = profileConfig.FinalPort, Username = profileConfig.FinalUser, DbName = profileConfig.FinalDbName, Password = profileConfig.FinalPassword };
            HideRowCounts = profileConfig.HideRowCounts;
        }

        public ProfileConfigModel(ProfileConfigModel profileConfigModel)
        {
            SetProperties(profileConfigModel);
        }

        public void SetProperties(ProfileConfigModel profileConfigModel)
        {
            Id = profileConfigModel.Id;
            Name = profileConfigModel.Name;
            DefaultMandtCol = profileConfigModel.DefaultMandtCol;
            DefaultBukrsCol = profileConfigModel.DefaultBukrsCol;
            DefaultGjahrCol = profileConfigModel.DefaultGjahrCol;
            ThreadsNumber = profileConfigModel.ThreadsNumber;
            SourceConnection = new DbConfig() { DbType = profileConfigModel.SourceConnection.DbType, Hostname = profileConfigModel.SourceConnection.Hostname, Port = profileConfigModel.SourceConnection.Port, Username = profileConfigModel.SourceConnection.Username, DbName = profileConfigModel.SourceConnection.DbName, Password = profileConfigModel.SourceConnection.Password};
            ViewboxConnection = new DbConfig() { DbType = profileConfigModel.ViewboxConnection.DbType, Hostname = profileConfigModel.ViewboxConnection.Hostname, Port = profileConfigModel.ViewboxConnection.Port, Username = profileConfigModel.ViewboxConnection.Username, DbName = profileConfigModel.ViewboxConnection.DbName, Password = profileConfigModel.ViewboxConnection.Password};
            FinalConnection = new DbConfig() { DbType = profileConfigModel.FinalConnection.DbType, Hostname = profileConfigModel.FinalConnection.Hostname, Port = profileConfigModel.FinalConnection.Port, Username = profileConfigModel.FinalConnection.Username, DbName = profileConfigModel.FinalConnection.DbName, Password = profileConfigModel.FinalConnection.Password};
            HideRowCounts = profileConfigModel.HideRowCounts;
        }

        public ProfileConfigModel Clone()
        {
            return new ProfileConfigModel(this);
        }
    }
}
