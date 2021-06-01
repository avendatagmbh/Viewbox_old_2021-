using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutomationWPF.Models;
using System.Collections.ObjectModel;

namespace AutomationWPF.ViewModels
{
    public class InfoViewModel : BaseViewModel
    {
        public InfoViewModel()
        {
            CurrentInfoModel = new InfoModel();
        }

        private InfoModel o_CurrentInfoModel;
        public InfoModel CurrentInfoModel
        {
            get
            {
                return o_CurrentInfoModel;
            }
            set
            {
                if (o_CurrentInfoModel != value)
                {
                    o_CurrentInfoModel = value;

                    RaisePropertyChanged(() => CurrentInfoModel);
                }
            }
        }
    }
}
