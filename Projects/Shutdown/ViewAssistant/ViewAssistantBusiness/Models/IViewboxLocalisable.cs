using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbAccess;
using SystemDb;

namespace ViewAssistantBusiness.Models
{
    public interface IViewboxLocalisable
    {
        event ConfigureLocalisationTextsClicked ConfigureLocalisationTextsClicked;

        void OnConfigureLocalisationTextsClicked(object sender, IViewboxLocalisable model);

        IDataObject Info { get; }

        string Name { get; }
    }
}
