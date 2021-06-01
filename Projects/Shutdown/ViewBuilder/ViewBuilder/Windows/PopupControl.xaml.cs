using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ViewBuilder.Windows
{
    /// <summary>
    /// Interaction logic for PopupControl.xaml
    /// </summary>
    public partial class PopupControl : UserControl, INotifyPropertyChanged
    {
        public PopupControl(string headerText, string contentText, Delegate extMethod = null)
        {
            InitializeComponent();
            HeaderText = headerText;
            ContentText = contentText;
            ExtMethod = extMethod;
            btnExt.Visibility = ExtMethod == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public Delegate ExtMethod;

        private string _headerText;
        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                if (_headerText != value)
                {
                    _headerText = value;
                    OnPropertyChanged("HeaderText");
                }
            }
        }

        private string _contentText;
        public string ContentText
        {
            get { return _contentText; }
            set
            {
                if (_contentText != value)
                {
                    _contentText = value;
                    OnPropertyChanged("ContentText");
                }
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            ((Popup) Parent).IsOpen = false;
        }

        private void btnExt_Click(object sender, RoutedEventArgs e)
        {
            if (ExtMethod != null)
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, ExtMethod);
            ((Popup) Parent).IsOpen = false;
        }
        

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
    }
}
