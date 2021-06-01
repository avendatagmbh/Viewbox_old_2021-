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
using System.Windows.Navigation;
using System.Windows.Shapes;
using eBalanceKitBusiness.Reconciliation.ViewModels;

namespace eBalanceKit.Windows.Reconciliation {
    /// <summary>
    /// Interaction logic for CtlReferenceList.xaml
    /// </summary>
    public partial class CtlReferenceList : UserControl {
        public CtlReferenceList() {
            InitializeComponent();

            Loaded += delegate {
                Init();
            };
        }

        private void Init() {
            
        }

        #region events
        //--------------------------------------------------------------------------------
        public event RoutedEventHandler Cancel;
        private void OnCancel() {
            CancelChanges();
            if (Cancel != null) Cancel(this, new RoutedEventArgs());
        }

        public event RoutedEventHandler Ok;
        private void OnOk() {
            SaveChanges();
            if (Ok != null) Ok(this, new RoutedEventArgs());
        }
        //--------------------------------------------------------------------------------
        #endregion events

        private ReconciliationsModel Model { get { return DataContext as ReconciliationsModel; } }

        private void SaveChanges() {
            throw new NotImplementedException();
        }

        private void CancelChanges() {
            throw new NotImplementedException();
        }
    }
}
