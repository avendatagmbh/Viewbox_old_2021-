using DbAccess.Structures;

namespace ViewBuilderBusiness.Structures
{
    /// <summary>
    ///   Enumeration of all avaliable script source modes.
    /// </summary>
    public enum ScriptSourceMode
    {
        /// <summary>
        ///   Use directory based script source.
        /// </summary>
        Directory,

        /// <summary>
        ///   Use database based script source.
        /// </summary>
        Database
    }

    /// <summary>
    ///   Configuration of viewscript sources.
    /// </summary>
    public class ConfigScriptSource
    {
        /// <summary>
        ///   Initializes a new instance of the <see cref="ConfigScriptSource" /> class.
        /// </summary>
        public ConfigScriptSource()
        {
            ScriptSourceMode = ScriptSourceMode.Directory;
            //this.Directory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
            Directory = @"Q:\Großprojekte";
            IncludeSubdirectories = false;
            DbConfig = new DbConfig("MySQL");
        }

        #region properties

        /// <summary>
        ///   Gets or sets the use database script source.
        /// </summary>
        /// <value> The use database script source. </value>
        public ScriptSourceMode ScriptSourceMode { get; set; }

        /// <summary>
        ///   Gets or sets the viewscript base directory.
        /// </summary>
        /// <value> The viewscript directory. </value>
        public string Directory { get; set; }

        /// <summary>
        ///   Gets or sets the viewscript base directory.
        /// </summary>
        /// <value> The viewscript directory. </value>
        public string BilanzDirectory { get; set; }

        /// <summary>
        ///   Gets or sets the viewscript base directory.
        /// </summary>
        /// <value> The viewscript directory. </value>
        public string ExtendedColumnInformationDirectory { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether viescripts should be search in all subfolders.
        /// </summary>
        /// <value> <c>true</c> if viescripts should be search in all subfolders; otherwise, <c>false</c> . </value>
        public bool IncludeSubdirectories { get; set; }

        /// <summary>
        ///   Gets or sets the database config for the viewscript database.
        /// </summary>
        /// <value> The db config. </value>
        public DbConfig DbConfig { get; set; }

        #endregion properties
    }
}