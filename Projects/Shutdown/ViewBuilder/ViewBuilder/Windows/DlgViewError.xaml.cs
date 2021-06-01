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

namespace ViewBuilder.Windows
{
    /// <summary>
    /// Interaction logic for DlgViewError.xaml
    /// </summary>
    public partial class DlgViewError : Window
    {
        public DlgViewError(string text)
        {
            Text = text;
            DataContext = this;
            InitializeComponent();
        }

        public string Text { get; private set; }
    }
}
