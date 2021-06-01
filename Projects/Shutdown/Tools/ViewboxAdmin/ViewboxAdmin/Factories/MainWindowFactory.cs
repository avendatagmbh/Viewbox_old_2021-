using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewboxAdmin.Manager;
using ViewboxAdmin.Models;
using ViewboxAdmin.Structures;
using ViewboxAdmin.ViewModels;
using ViewboxAdminBusiness.Factories;

namespace ViewboxAdmin.Factories
{
    public static class MainWindowFactory
    {
        public static  MainWindow_ViewModel Create(NavigationTree navTree) {
            return new MainWindow_ViewModel(ProfileManagerFactory.Create(),new ProfileModelRepository(),new NavigationEntriesDataContext(navTree),new Dispatcher(),null);
        }
    }
}
