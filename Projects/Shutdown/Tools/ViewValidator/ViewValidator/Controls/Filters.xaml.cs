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

namespace ViewValidator.Controls {
    /// <summary>
    /// Interaktionslogik für Filters.xaml
    /// </summary>
    public partial class Filters : UserControl {
        public Filters() {
            InitializeComponent();
        }

        private void tbFilterView_TextChanged(object sender, TextChangedEventArgs e) {
            tbFilterView.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void tbFilterValidation_TextChanged(object sender, TextChangedEventArgs e) {
            tbFilterValidation.GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }
    }
}
