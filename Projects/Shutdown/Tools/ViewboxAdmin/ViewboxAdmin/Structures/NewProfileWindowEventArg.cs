using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewboxAdmin.ViewModels;

namespace ViewboxAdmin.Structures
{
    public class NewProfileWindowEventArg: EventArgs
    {
        public NewProfileWindowEventArg(CreateNewProfile_ViewModel createnewprofileVM) { this.CreateNewProfileVM = createnewprofileVM; }

        public CreateNewProfile_ViewModel CreateNewProfileVM { get; private set; }
    }
}
