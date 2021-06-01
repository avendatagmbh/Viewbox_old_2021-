/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Attila Papp        2012-09-04        Managing profile IO manipulations. Fully tested code. Reviewed legacy code from Mirko.
 * 
 *************************************************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.IO;
using DbAccess.Structures;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewboxAdminBusiness.Manager
{
    public class ProfileManager:IProfileManager
    {
        #region constructors
        public ProfileManager(IApplicationManager appmanager, IFileManager filemanager, IXmlProfileManager xmlprofilemanager) {
             this._applicationManager = appmanager;
             this._filemanager = filemanager;
             this._xmlprofilemanager = xmlprofilemanager;
             Profiles = new ObservableCollection<IProfile>();
         }
        #endregion constructors

        #region Properties
        private IApplicationManager _applicationManager;
        private IFileManager _filemanager;
        private IXmlProfileManager _xmlprofilemanager;
        public IApplicationManager ApplicationManager { get { return _applicationManager; } }
        public string ProfileDirectory {
            get { return _applicationManager.ApplicationConfig.ConfigDirectory + "\\ViewboxAdmin\\profiles"; }
        }
        public ObservableCollection<IProfile> Profiles { get; private set; }
        public IFileManager FileManager { get { return _filemanager; } }
        #endregion Properties

        #region Methods

        /// <summary>
        /// Save the profile to an xml file, and add it to the profile collection
        /// </summary>
        /// <param name="profile">proile to save</param>
        public void Save(IProfile profile) {
                    if (!FileManager.DirectoryExist(ProfileDirectory)) {
                        FileManager.CreateDirectory(ProfileDirectory);
                    }
                    _xmlprofilemanager.SaveProfileToXml(profile, FullFileNamePath(profile.Name));
                    Profiles.Add(profile);
        }

        public IProfile Open(string profileName) {
            //getting an empty profile should be in a factory like method
            IProfile profile = new Profile(new SystemDb.SystemDb(), new DbConfig("MySQL"));

            string file = FullFileNamePath(profileName);
            try
            {
                _xmlprofilemanager.LoadProfileFromFile(ref profile, file);

            }
            catch (Exception ex)
            {
                //remove the broken file
                profile.Name = profileName;
                DeleteProfile(profile);
                throw new Exception("Konnte das Profil " + profileName + " nicht laden. Profil wird gelöscht", ex);
            }

            return profile;
        }

        /// <summary>
        /// remove profile from the files and from the profile collection
        /// </summary>
        /// <param name="profile"></param>
        public void DeleteProfile(IProfile profile) {
            string profileName = profile.Name;

            string filename = FullFileNamePath(profileName);
            if (FileManager.FileExist(filename))
            {
                FileManager.Delete(filename);
            }
            Profiles.Remove(profile);
        }

        public IProfile GetProfile(string name) {
            foreach (var profile in Profiles)
                if (profile.Name.ToLower() == name.ToLower())
                    return profile;
            return null;
        }

        public void SetLastProfile(string profile) { ApplicationManager.ApplicationConfig.LastProfile = profile; }

        public IProfile GetLastProfile() { return GetProfile(this.ApplicationManager.ApplicationConfig.LastProfile); }

        public void LoadProfiles() {
            Profiles.Clear();

            if (FileManager.DirectoryExist(ProfileDirectory))
            {
                foreach (FileInfo fi in FileManager.GetFileInfo(ProfileDirectory))
                {
                    if (fi.Name.EndsWith(".xml"))
                    {
                        string name = fi.Name.Substring(0, fi.Name.Length - 4);
                        IProfile profile = Open(name);
                        Profiles.Add(profile);
                    }
                }
            }

        }

        private string FullFileNamePath(string profilename) {
            return ProfileDirectory + "\\" + profilename + ".xml";
        }

        #endregion Methods
    }
}
