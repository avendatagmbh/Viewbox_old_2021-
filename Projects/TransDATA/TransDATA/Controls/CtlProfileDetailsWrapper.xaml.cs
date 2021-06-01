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

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlProfileDetailsWrapper.xaml
    /// </summary>
    public partial class CtlProfileDetailsWrapper : UserControl {
        public CtlProfileDetailsWrapper(UserControl controlToAdd, string header) {
            InitializeComponent();
            insertBorder.Child = controlToAdd;
            txtHeader.Text = header;
        }
    }
}
