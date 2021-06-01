/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Attila Papp        2012-09-03      fully unit test covered class, legacy code from Mirko to some extent
 *************************************************************************************************************/
using System;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewboxAdminBusiness.Manager
{
    /// <summary>
    /// This class manages application configuration load/save. Fully unit tested.
    /// </summary>
    public class ApplicationConfigManager : IApplicationManager
    {
        public ApplicationConfigManager(IFileManager filemanager, IXmlConfigManager xmlmanager) {
            this.ApplicationConfig = new ApplicationConfig(new FileManager());
            this.FileManager = filemanager;
            this.XmlManager = xmlmanager;
        }

        private readonly string _configfilename = "viewboxadmin.xml";

        public  string ConfigFileName {
            get { return _configfilename; }
        }

        private readonly string _configdirectoryrelativepath = "\\AvenDATA\\ViewboxAdmin";

        public string ConfigDirectory {
            get {
                string appdata = _filemanager.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string path =  appdata + _configdirectoryrelativepath;
                             
                if (!_filemanager.DirectoryExist(path)) {
                    _filemanager.CreateDirectory(path);
                }
                return path;
            }
        }

        public string ConfigPath { get { return ConfigDirectory + "\\" + ConfigFileName; } }

        private IFileManager _filemanager;
        public IFileManager FileManager {
            get { return _filemanager; }
            set { _filemanager = value; }
        }

        private IApplicationConfig _applicationconfig;

        public IApplicationConfig ApplicationConfig {
            get { return _applicationconfig; } 
            set { _applicationconfig = value; } }
        public IXmlConfigManager XmlManager { get; set; }

        public void Load() {
            if (IsConfigFileExist()) {

                ApplicationConfig=XmlManager.LoadApplicationConfigurationDataFromXml(ConfigPath);
            }
        }

        public void Save() {
            XmlManager.SaveApplicationConfigurationDataToXml(this.ApplicationConfig, ConfigPath);
        }

        private bool IsConfigFileExist() {
            return _filemanager.FileExist(ConfigPath);
        }

        

        

        
    }
}
