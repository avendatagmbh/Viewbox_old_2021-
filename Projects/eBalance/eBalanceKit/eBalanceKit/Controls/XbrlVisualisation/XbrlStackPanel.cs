using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace eBalanceKit.Controls.XbrlVisualisation {
    
    internal class XbrlStackPanel : XbrlBasePanel {

        public XbrlStackPanel() {
            this.ContentPanel = new StackPanel();
            base.ContentPanel.Children.Add(this.ContentPanel);
        }

        public new Panel ContentPanel { get; private set; }
    }
}
