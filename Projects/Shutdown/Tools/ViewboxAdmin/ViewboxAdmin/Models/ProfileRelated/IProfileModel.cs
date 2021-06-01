using System;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using Utils;
using ViewboxAdmin.Models.ViewboxDb;
using ViewboxAdminBusiness.Manager;
using ViewboxAdmin_ViewModel.Structures.Config;
using ErrorEventArgs = System.IO.ErrorEventArgs;

namespace ViewboxAdmin.Models.ProfileRelated {
    public interface IProfileModel : INotifyPropertyChanged {
        event EventHandler<ErrorEventArgs> FinishedProfileLoadingEvent;
        event EventHandler<ErrorEventArgs> Error;
        IProfile Profile { get;  }
        //TODO investigate this...
        IProgressCalculator LoadingProgress { get; set; }
        IProfilePartLoadingEnumHelper ProfilePartLoadingEnumHelper { get; }
        void LoadData();
        Dictionary<SystemDb.SystemDb.Part,string> LoadingCompletedPartDictionary { get; }
        
    }
}