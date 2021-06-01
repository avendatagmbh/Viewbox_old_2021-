using System;
using System.Windows;
using System.Windows.Controls;
using DbAccess;
using ViewValidator.Models.Profile;

namespace ViewValidator.Controls.Profile {
    /// <summary>
    /// Interaktionslogik für TableMappingDetails.xaml
    /// </summary>
    public partial class TableMappingDetails : UserControl {
        public TableMappingDetails(Window owner) {
            InitializeComponent();
            this.Owner = owner;
        }

        ProfileModel Model { get { return DataContext as ProfileModel; } }
        Models.Profile.DataPreviewModel DataPreviewModel { get; set; }

        private void DetailTab_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (e.AddedItems.Count != 1 || Model == null || Model.SelectedColumnMappingModel == null) return;
            TabItem selectedItem = e.AddedItems[0] as TabItem;
            if (selectedItem == ColumnMappingItem) {
                //FillColumnMapping();
            } else if (selectedItem == ColumnMappingItem) {
                FillSortMapping();
            } else if (selectedItem == DataPreviewItem) {
                FillDataPreview();
            } else if (selectedItem == StoredProcedureItem) {
                CheckForStoredProcedure();
            }

        }

        private void CheckForStoredProcedure() {
            if (Model.SelectedTableMapping == null || Model.SelectedColumnMappingModel == null) return;
            try {
                Model.SelectedColumnMappingModel.StoredProcedureModel.CheckForDynamicView(Model.SelectedTableMapping);
                storedProcedureCtl.FillParameterGrid(Model.SelectedTableMapping);

            } catch (InvalidOperationException ex) {
                //This error happens if the table is not in the system table.
                Console.WriteLine("Fehler: " + Environment.NewLine + ex.Message);
            } catch (Exception ex) {
                MessageBox.Show(Owner, "Fehler: " + Environment.NewLine + ex.Message);
            }
        }

        private void FillSortMapping() {
            //if (Model.SelectedColumnMappingModel.Sort.Count == 0 && Model.SelectedColumnMappingModel.Mapping.Count != 0) {
            //    Model.SelectedColumnMappingModel.Sort.Add(Model.SelectedColumnMappingModel.Mapping[0]);
            //}
        }

        private void FillColumnMapping() {
            if (Model.SelectedTableMapping == null) return;
            string TableSource = Model.SelectedTableMapping.TableValidation.Name;
            string TableDestination = Model.SelectedTableMapping.TableView.Name;
            if (TableSource == null || TableDestination == null) return;

            try {
                using (IDatabase view = ConnectionManager.CreateConnection(Model.SelectedTableMapping.TableView.DbConfig)) {
                    using (IDatabase access = ConnectionManager.CreateConnection(Model.SelectedTableMapping.TableValidation.DbConfig)) {
                        view.Open();
                        access.Open();

                        Model.SelectedColumnMappingModel.ObsDestination.Clear();
                        foreach (string col in view.GetColumnNames(Model.DatabaseModel.ConfigDest.DbName, TableDestination)) {
                            Model.SelectedColumnMappingModel.ObsDestination.Add(col);
                        }
                        
                        Model.SelectedColumnMappingModel.ObsSource.Clear();
                        if (!access.TableExists(TableSource)) {
                            throw new InvalidOperationException("Die Tabelle \"" + TableSource +
                                                                "\" existiert nicht in der Datenbank " +
                                                                Model.SelectedTableMapping.TableValidation.DbConfig.Hostname);
                        }
                        foreach (string col in access.GetColumnNames(TableSource)) {
                            Model.SelectedColumnMappingModel.ObsSource.Add(col);
                        }
                    }
                }

                //Only use predefined mapping if there is none
                if (Model.SelectedColumnMappingModel.Mapping.Count == 0)
                    Model.SelectedColumnMappingModel.AutomaticAssign();
                else
                    Model.SelectedColumnMappingModel.UseMapping();
            } catch (Exception ex) {
                MessageBox.Show(Owner, "Ein Fehler ist aufgetreten:" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillDataPreview(){
            try {
                DataPreviewModel.FillData(true);
                dataPreviewCtl.DataContext = DataPreviewModel;
                dataPreviewCtl.Update();
            } catch (Exception ex) {
                MessageBox.Show("Ein Fehler ist aufgetreten beim Anzeigen der Daten: " + Environment.NewLine + ex.Message);
            }
        }

        public void Update() {
            FillColumnMapping();
            FillSortMapping();
            columnMappingCtl.DataContext = Model.SelectedColumnMappingModel;
            sortMappingCtl.DataContext = Model.SelectedColumnMappingModel;
            storedProcedureCtl.DataContext = Model.SelectedColumnMappingModel.StoredProcedureModel;
            CheckForStoredProcedure();
            //filterCtl.DataContext = Model.SelectedColumnMappingModel;
        }

        public Window Owner { get; set; }

        internal void SetDataPreviewModel(Models.Profile.DataPreviewModel dataPreviewModel) {
            this.DataPreviewModel = dataPreviewModel;
            if (DetailTab.SelectedItem == DataPreviewItem) 
                FillDataPreview();
        }
    }
}
