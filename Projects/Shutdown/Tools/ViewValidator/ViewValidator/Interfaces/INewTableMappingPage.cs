using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewValidator.Interfaces {
    interface INewTableMappingPage {
        string ValidationError();
        string SelectedTable();
    }
}
