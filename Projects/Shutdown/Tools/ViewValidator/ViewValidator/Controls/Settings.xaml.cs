using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ViewValidator.Controls.Rules;
using ViewValidator.Models;
using ViewValidator.Models.Rules;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Controls {
    /// <summary>
    /// Interaktionslogik für RuleSelection.xaml
    /// </summary>
    public partial class Settings : UserControl {
        public Settings() {
            InitializeComponent();

        }

        public SettingsModel Model { get { return DataContext as SettingsModel; } }

        public ObservableCollection<TableMapping> TableMappings { 
            get { 
                if (Model == null) return null;
                return Model.TableMappings;
            }
 
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            //if (TableMappings != null) {
            //    int id = 0;
            //    mainPanel.Children.RemoveRange(1, mainPanel.Children.Count - 1);
            //    foreach (var tableMapping in TableMappings) {
            //        mainPanel.Children.Add(new RuleSelectionDetails() { DataContext = new RuleSelectionDetailsModel(tableMapping, id++) });
            //    }
            //}
        }
    }
}
