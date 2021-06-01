using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using ViewboxAdmin_ViewModel.Structures.Config;
namespace ViewboxAdminBusiness.Manager
{
    public interface IProfileManager {

        /// <summary>
        /// This interface abstracts all the profile save/load operations.
        /// </summary>
        /// <param name="profile"></param>

        void Save(IProfile profile);
        void DeleteProfile(IProfile profile);
        void LoadProfiles();
        string ProfileDirectory { get; }
        IProfile GetProfile(string name);
        IProfile GetLastProfile();
        void SetLastProfile(string profile );

        ObservableCollection<IProfile> Profiles { get; }
        IApplicationManager ApplicationManager { get; }
        IFileManager FileManager { get; }
        IProfile Open(string profileName);

    }
}
