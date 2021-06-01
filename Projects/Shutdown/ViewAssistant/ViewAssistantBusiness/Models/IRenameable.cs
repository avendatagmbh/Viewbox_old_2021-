using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;

namespace ViewAssistantBusiness.Models
{
    public interface IRenameable
    {
        IDataObject Info { get; }

        string Name { get; set; }

    }
}
