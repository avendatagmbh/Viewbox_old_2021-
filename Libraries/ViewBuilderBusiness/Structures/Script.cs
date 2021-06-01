using System.Collections.Generic;
using System.IO;
using ProjectDb.Tables;
using ViewBuilderBusiness.Persist;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilderBusiness.Structures
{
    /// <summary>
    ///   This class represents a viewscript file.
    /// </summary>
    public class ScriptFile
    {
        internal ProfileConfig Profile;

        /// <summary>
        ///   Initializes a new instance of the <see cref="ScriptFile" /> class.
        /// </summary>
        /// <param name="file"> The file. </param>
        public ScriptFile(FileInfo file, ProfileConfig Profile)
        {
            File = file;
            Views = new List<Viewscript>();
            this.Profile = Profile;
            Load();
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="ScriptFile" /> class.
        /// </summary>
        protected ScriptFile()
        {
            Views = new List<Viewscript>();
        }

        /// <summary>
        ///   Gets or sets the views.
        /// </summary>
        /// <value> The views. </value>
        public List<Viewscript> Views { get; private set; }

        /// <summary>
        ///   Name of the viewscript file.
        /// </summary>
        public FileInfo File { get; set; }

        /// <summary>
        ///   Loads this instance.
        /// </summary>
        private void Load()
        {
            Views = ViewscriptParser.Parse(File, Profile);
        }
    }
}