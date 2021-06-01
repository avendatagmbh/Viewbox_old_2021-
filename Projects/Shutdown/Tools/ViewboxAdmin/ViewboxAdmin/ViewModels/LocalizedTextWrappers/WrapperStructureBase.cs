using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels
{
    public abstract class WrapperStructureBase 
    {
        protected WrapperStructureBase(ILanguage language, ISystemDb systemdb) { 
            this.SystemDb = systemdb;
            this.Language = language;
        }

        public ISystemDb SystemDb { get; private set; }

        public ILanguage Language { get; private set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));

        }
    }
}
