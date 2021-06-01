/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-11-19      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TransDATABusiness.ConfigDb.Tables;

namespace TransDATABusiness.Structures {

    /// <summary>
    /// Application config class.
    /// </summary>
    public class AppConfig : IDisposable {

        /// <summary>
        /// Initializes a new instance of the <see cref="AppConfig"/> class.
        /// </summary>
        internal AppConfig() {
            if (!System.IO.Directory.Exists(ConfigDbPath)) {
                System.IO.Directory.CreateDirectory(ConfigDbPath);
            }
            
            this.ConfigDb = new ConfigDb.ConfigDb();
            this.ConfigDb.Init(ConfigDbPath);
        }

        /// <summary>
        /// Gets or sets the interface for the config database.
        /// </summary>
        /// <value>The interface for the config database.</value>
        internal ConfigDb.ConfigDb ConfigDb { get; set; }

        /// <summary>
        /// Checks the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public bool CheckPassword(string password) {
            return this.ConfigDb.CheckPassword(password);
        }

        /// <summary>
        /// Gets the config db path.
        /// </summary>
        /// <value>The config db path.</value>
        private string ConfigDbPath {
            get { return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "\\AvenDATA\\TransDATA"; }
        }

        /// <summary>
        /// Disposes this instance.
        /// </summary>
        public void Dispose() {
            ConfigDb.Dispose();
        }


        public void GetStructure(object profile) {
            SourceDb.SourceDb.GetStructure((Profile)profile);
        }

    }
}
