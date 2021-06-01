using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace eBalanceKit.Controls.BalanceList {
    public class BalListImpAssistPageBase : UserControl {
        public virtual bool Validate() { return true; }

    }
}
