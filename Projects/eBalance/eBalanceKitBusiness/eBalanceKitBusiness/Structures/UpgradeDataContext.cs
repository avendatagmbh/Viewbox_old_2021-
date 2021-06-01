using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace eBalanceKitBusiness.Structures {
    public class UpgradeDataContext {

        public ObservableCollection<UpgradeRow> Rows { get; set; }

        public string Value { get; set; }

        public string Header { get; set; }

        public DateTime PossibleLastLogin { get; set; }
    }
}
