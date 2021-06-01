using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utils;
using Binding = System.Windows.Data.Binding;
using CheckBox = System.Windows.Controls.CheckBox;
using UserControl = System.Windows.Controls.UserControl;

namespace DatabaseManagement.Controls {
    /// <summary>
    /// Interaktionslogik für CtlAutomaticBackup.xaml
    /// </summary>
    public partial class CtlAutomaticBackup : UserControl {
        public CtlAutomaticBackup() {
            InitializeComponent();
            CreateCheckBoxes();
            DataContext = new Models.AutomaticBackupModel();
        }

        private Models.AutomaticBackupModel Model { get { return DataContext as Models.AutomaticBackupModel; } }

        private void CreateCheckBoxes() {
            DayOfWeek firstDay = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            for (int dayIndex = 0; dayIndex < 7; dayIndex++) {
                var currentDay = (DayOfWeek)(((int)firstDay + dayIndex) % 7);
                
                CheckBox cb = new CheckBox();
                cb.Content = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)currentDay];
                cb.Name = "cb" + dayIndex;
                cb.Margin = new Thickness(5,0,0,0);
                cb.SetBinding(CheckBox.IsCheckedProperty, new Binding("WeekDayChecker[" + currentDay + "]"));

                Grid.SetColumn(cb, dayIndex);
                
                //CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)currentDay];

                //daysOfWeek.Children.Add(new CheckBox { Content = CultureInfo.CurrentCulture.DateTimeFormat.DayNames[(int)currentDay] });
                daysOfWeek.Children.Add(cb);
            }

        }

        private void BtnSelectFolderPath(object sender, RoutedEventArgs e) {
            var dlgFolder = new FolderBrowserDialog {
                ShowNewFolderButton = true
            };
            if (!string.IsNullOrEmpty(Model.SaveFolder)) {
                dlgFolder.SelectedPath = Model.SaveFolder;
            }

            if (dlgFolder.ShowDialog() == DialogResult.OK) {
                Model.SaveFolder = dlgFolder.SelectedPath;
                Model.Config.File = dlgFolder.SelectedPath;
            }
        }
    }
}
