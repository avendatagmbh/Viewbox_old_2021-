using DbAccess;

namespace ViewAssistantBusiness.Config
{
    [DbTable("ProfileConfig")]
    public class ProfileConfig
    {
        public ProfileConfig()
        {
            
        }

        public ProfileConfig(ProfileConfigModel profileConfigModel)
        {
            SetProfileConfigDatas(profileConfigModel);
        }

        public void SetProfileConfigDatas(ProfileConfigModel profileConfigModel)
        {
            Id = profileConfigModel.Id;
            Name = profileConfigModel.Name;
            DefaultMandtCol = profileConfigModel.DefaultMandtCol;
            DefaultBukrsCol = profileConfigModel.DefaultBukrsCol;
            DefaultGjahrCol = profileConfigModel.DefaultGjahrCol;
            ThreadsNumber = profileConfigModel.ThreadsNumber;

            SourceDbType = profileConfigModel.SourceConnection.DbType;
            SourceHost = profileConfigModel.SourceConnection.Hostname;
            SourcePort = profileConfigModel.SourceConnection.Port;
            SourceUser = profileConfigModel.SourceConnection.Username;
            SourceDbName = profileConfigModel.SourceConnection.DbName;
            SourcePassword = profileConfigModel.SourceConnection.Password;

            ViewboxDbType = profileConfigModel.ViewboxConnection.DbType;
            ViewboxHost = profileConfigModel.ViewboxConnection.Hostname;
            ViewboxPort = profileConfigModel.ViewboxConnection.Port;
            ViewboxUser = profileConfigModel.ViewboxConnection.Username;
            ViewboxDbName = profileConfigModel.ViewboxConnection.DbName;
            ViewboxPassword = profileConfigModel.ViewboxConnection.Password;

            FinalDbType = profileConfigModel.FinalConnection.DbType;
            FinalHost = profileConfigModel.FinalConnection.Hostname;
            FinalPort = profileConfigModel.FinalConnection.Port;
            FinalUser = profileConfigModel.FinalConnection.Username;
            FinalDbName = profileConfigModel.FinalConnection.DbName;
            FinalPassword = profileConfigModel.FinalConnection.Password;

            HideRowCounts = profileConfigModel.HideRowCounts;
        }

        [DbColumn("id"), DbPrimaryKey]
        public long Id { get; set; }

        [DbColumn("name")]
        public string Name { get; set; }

        [DbColumn("DefaultMandtCol")]
        public string DefaultMandtCol { get; set; }

        [DbColumn("DefaultBukrsCol")]
        public string DefaultBukrsCol { get; set; }

        [DbColumn("DefaultGjahrCol")]
        public string DefaultGjahrCol { get; set; }

        [DbColumn("ThreadsNumber")]
        public int ThreadsNumber { get; set; }

        [DbColumn("SourceDbType")]
        public string SourceDbType { get; set; }

        [DbColumn("SourceHost")]
        public string SourceHost { get; set; }

        [DbColumn("SourcePort")]
        public int SourcePort { get; set; }

        [DbColumn("SourceUser")]
        public string SourceUser { get; set; }

        [DbColumn("SourceDbName")]
        public string SourceDbName { get; set; }

        [DbColumn("SourcePassword")]
        public string SourcePassword { get; set; }


        [DbColumn("ViewboxDbType")]
        public string ViewboxDbType { get; set; }

        [DbColumn("ViewboxHost")]
        public string ViewboxHost { get; set; }

        [DbColumn("ViewboxPort")]
        public int ViewboxPort { get; set; }

        [DbColumn("ViewboxUser")]
        public string ViewboxUser { get; set; }

        [DbColumn("ViewboxDbName")]
        public string ViewboxDbName { get; set; }

        [DbColumn("ViewboxPassword")]
        public string ViewboxPassword { get; set; }



        [DbColumn("FinalDbType")]
        public string FinalDbType { get; set; }

        [DbColumn("FinalHost")]
        public string FinalHost { get; set; }

        [DbColumn("FinalPort")]
        public int FinalPort { get; set; }

        [DbColumn("FinalUser")]
        public string FinalUser { get; set; }

        [DbColumn("FinalDbName")]
        public string FinalDbName { get; set; }

        [DbColumn("FinalPassword")]
        public string FinalPassword { get; set; }

        [DbColumn("HideRowCounts")]
        public bool HideRowCounts { get; set; }
    }
}
