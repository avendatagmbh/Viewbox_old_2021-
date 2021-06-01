using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utils;
using eBalanceKitBusiness.Import;

namespace eBalanceKit.Controls.BalanceList {
    /// <summary>
    /// Interaktionslogik für CtlExampleCsv.xaml
    /// </summary>
    public partial class CtlExampleCsv : INotifyPropertyChanged {
        public CtlExampleCsv() {
            InitializeComponent();
        }

        private PropertyChangedEventHandler _propertyChanged;
        public event PropertyChangedEventHandler PropertyChanged {
            add { this._propertyChanged += value; }
            remove { this._propertyChanged -= value; }
        }
        
        protected virtual void OnPropertyChanged(string propertyName) {
            if (_propertyChanged != null) _propertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) {
            UIHelpers.TryFindParent<Window>(this).Close();
        }
    }
}
