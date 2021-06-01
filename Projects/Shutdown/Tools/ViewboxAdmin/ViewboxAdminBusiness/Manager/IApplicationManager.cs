using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewboxAdmin_ViewModel.Structures.Config;

namespace ViewboxAdminBusiness.Manager
{
    public interface IApplicationManager {
        /// <summary>
        /// load the application config data
        /// </summary>
        void Load();
        /// <summary>
        /// save the application config data
        /// </summary>
        void Save();
        IApplicationConfig ApplicationConfig { get; set; }
    }
}
