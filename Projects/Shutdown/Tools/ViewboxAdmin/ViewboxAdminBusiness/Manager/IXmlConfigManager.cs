using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewboxAdmin_ViewModel.Structures.Config;


namespace ViewboxAdminBusiness.Manager
{
    public interface IXmlConfigManager {
        IApplicationConfig LoadApplicationConfigurationDataFromXml(string path);
        void SaveApplicationConfigurationDataToXml(IApplicationConfig appconfig, string path);
    }
}
