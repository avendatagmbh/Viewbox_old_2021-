using System.Windows;
using DbAccess.Structures;
using ViewValidator.Interfaces;
using ViewValidator.Models.Profile;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Windows {
    /// <summary>
    /// Interaktionslogik für DlgNewTableMapping.xaml
    /// </summary>
    public partial class DlgNewTableMapping : Window {
        public DlgNewTableMapping(TableMapping newMapping) {
            InitializeComponent();
            this.NewMapping = newMapping;
        }

        private TableMappingModel Model { get { return DataContext as TableMappingModel; } }
        private TableMapping NewMapping { get; set; }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            //if (lbSource.SelectedIndex == -1) { 
            //    MessageBox.Show("Bitte eine Tabelle aus der Verprobungsdatenbank auswählen.");
            //    return;
            //}

            //if (lbDestination.SelectedIndex == -1) {
            //    MessageBox.Show("Bitte eine Tabelle aus der Viewdatenbank auswählen.");
            //    return;
            //}

            //NewMapping.TableValidation = new Table(lbSource.SelectedItem as string);
            //NewMapping.TableView = new Table(lbDestination.SelectedItem as string);

            //if (Model.ContainsMapping(NewMapping)) {
            //    MessageBox.Show("Das Mapping existiert bereits.");
            //    return;
            //}

            //Model.AddMapping(NewMapping);

            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }

        INewTableMappingPage CurrentTableMappingPage { 
            get { 
                if(Model.CurrentPage == 1)
                    return ctlPage1;
                return ctlPage2; 
            } 
        }

        bool HasValidationError() {
            string error = CurrentTableMappingPage.ValidationError();
            if(!string.IsNullOrEmpty(error)){
                MessageBox.Show(Owner, error, "", MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return true;
            }
            return false;
        }

        private void btnNext_Click(object sender, RoutedEventArgs e) {
            if (HasValidationError()) return;
            Model.CurrentPage++;
        }

        private void btnPrevious_Click(object sender, RoutedEventArgs e) {
            Model.CurrentPage--;
        }

        private void btnFinish_Click(object sender, RoutedEventArgs e) {
            if (HasValidationError()) return;
            NewMapping.TableView = new Table(ctlPage1.SelectedTable(), (DbConfig) Model.DbConfigView.Clone());
            NewMapping.TableValidation = new Table(ctlPage2.SelectedTable(), (DbConfig) Model.SelectedValidationConfig.Clone());

            if (Model.ContainsMapping(NewMapping)) {
                MessageBox.Show(Owner, "Das Mapping existiert bereits, bitte wählen Sie andere Tabellen aus.", "", MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            Model.AddMapping(NewMapping);

            this.DialogResult = true;
            this.Close();
        }
    }
}
