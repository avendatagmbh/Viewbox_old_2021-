using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutomationWPF.Helpers;
using System.Reflection;

namespace AutomationWPF.Models
{
    public class InfoModel : NotificationObject
    {
        private string o_Version;
        public string Version
        {
            get
            {
                if (string.IsNullOrWhiteSpace(o_Version))
                {
                    o_Version = Assembly.GetEntryAssembly().GetName().Version.ToString(4);
                    RaisePropertyChanged(() => Version);
                }                

                return o_Version;
            }
        }
    }    
}
