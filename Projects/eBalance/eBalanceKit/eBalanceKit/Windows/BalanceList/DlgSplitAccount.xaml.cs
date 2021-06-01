using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace eBalanceKit.Windows.BalanceList {
    
    /// <summary>
    /// Interaktionslogik für DlgSplitAccount.xaml
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-06-21</since>
    public partial class DlgSplitAccount : Window {
        
        public DlgSplitAccount(Window owner) {
            InitializeComponent();
            Owner = owner;
            
            ctlSplitAccount.Ok += new EventHandler(ctlSplitAccount_Ok);
            ctlSplitAccount.Cancel += new EventHandler(ctlSplitAccount_Cancel);
        }

        void ctlSplitAccount_Cancel(object sender, System.EventArgs e) {
            UnregisterEventHandler();
            this.DialogResult = false;
        }

        void ctlSplitAccount_Ok(object sender, System.EventArgs e) {
            UnregisterEventHandler();
            this.DialogResult = true;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { 
            DragMove(); 
        }

        private void UnregisterEventHandler() {
            ctlSplitAccount.Ok -= new EventHandler(ctlSplitAccount_Ok);
            ctlSplitAccount.Cancel -= new EventHandler(ctlSplitAccount_Cancel);
        }
    }
}
