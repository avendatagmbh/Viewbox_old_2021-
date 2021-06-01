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
using eBalanceKitBusiness;
using System.ComponentModel;
using eBalanceKitBusiness.Manager;
using eBalanceKit.Structures;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace eBalanceKit.Windows.BalanceList {

    #region DlgEditBalanceList
    //--------------------------------------------------------------------------------
    public partial class DlgEditBalanceList : Window {
        public DlgEditBalanceList(IBalanceList balanceList) {
            InitializeComponent();

            this.Model = new DlgEditBalanceListModel(this, balanceList);
            this.DataContext = this.Model;
        }


        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) { e.Handled = true; this.Close(); } }
        private void btnClose_Click(object sender, RoutedEventArgs e) { this.Close(); }
        private void btnReimportBalanceList_Click(object sender, RoutedEventArgs e) { Model.ReimportBalanceList(); }

        private void btnTemplates_Click(object sender, RoutedEventArgs e) {
            slideControl.ShowChild(0);
            //ShowTemplatesPanel();
            //DlgBalListTemplates dlg = new DlgBalListTemplates { Owner = this };
            //dlg.ShowDialog();
        }


        internal DlgEditBalanceListModel Model { get; set; }
    }

    //--------------------------------------------------------------------------------
    #endregion DlgEditBalanceList
    
    #region DlgEditBalanceListModel
    //--------------------------------------------------------------------------------
    internal class DlgEditBalanceListModel : INotifyPropertyChanged {
                
        public DlgEditBalanceListModel(Window owner, IBalanceList balanceList) {
            this.Owner = owner;
            this.BalanceList = balanceList;
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion

        public Window Owner { get; set; }
        public IBalanceList BalanceList { get; set; }

        public void ReimportBalanceList() {
            // TODO
        }

        internal void CreateAccountGroup() {
            var group = AccountGroupManager.CreateAccountGroup(BalanceList);
            
            group.Validate(); // init validation messages

            DlgGroupAccount dlg = new DlgGroupAccount {
                Owner = this.Owner,
                DataContext = group,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };

            bool? result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true) {
                group.DoDbUpdate = true;
                group.Save();
                BalanceList.AddAccountGroup(group);
            }
        }
    }
    //--------------------------------------------------------------------------------
    #endregion DlgSplittedAccountsOverviewModel

}
