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
using System.Windows.Forms;
using DbComparisonV2.Models;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace DbComparisonV2.Controls
{
    /// <summary>
    /// Interaktionslogik für CtlViewScript.xaml
    /// </summary>
    public partial class CtlViewScript : System.Windows.Controls.UserControl
    {
        public CtlViewScript()
        {
            InitializeComponent();
        }

        private void btnLoadViewScripts_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog(){ Multiselect = true  };

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                Config.ScriptFiles = new ObservableCollection<string>(fileDialog.FileNames);
            }
        }

        private ViewScriptConfig Config 
        {
            get { return DataContext as ViewScriptConfig; }
            set{ DataContext = value; }
        }

        private void btnRemoveFile_Click(object sender, RoutedEventArgs e)
        {
            // removes the current file
            Config.ScriptFiles.Remove(((System.Windows.Controls.Control)sender).DataContext.ToString());
        }
    }

    public class GetSafeFileNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return System.IO.Path.GetFileName(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
