using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewboxAdmin.ViewModels.LocalizedTextLoaders
{
    public abstract  class LoaderBase
    {
        public LoaderBase(ISystemDb systemDb) {
            this.SystemDb = systemDb;
        }

        public ISystemDb SystemDb { get; private set; }
    }
}
