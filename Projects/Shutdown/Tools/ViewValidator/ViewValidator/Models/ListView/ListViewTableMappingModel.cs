using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Windows;
using DbAccess;
using DbAccess.Structures;
using ViewValidator.Controls.Profile;
using ViewValidator.Manager;
using ViewValidator.Models.Profile;
using ViewValidator.Windows;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Models.ListView {
    public class ListViewTableMappingModel : ListViewModelBase, IListViewModel, INotifyPropertyChanged {

        #region Constructor
        public ListViewTableMappingModel(MainWindowModel mainWindowModel) {
            this.MainWindowModel = mainWindowModel;
            _items = mainWindowModel.TableMappings;
            mainWindowModel.PropertyChanged += new PropertyChangedEventHandler(mainWindowModel_PropertyChanged);
        }

        void mainWindowModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "SelectedProfile":
                    //Update table mappings when a new profile has been selected
                    _items = (sender as MainWindowModel).TableMappings;
                    OnPropertyChanged("Items");
                    break;
            }
        }

        #endregion

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        #region properties
        MainWindowModel MainWindowModel{get;set;}

        #region Items
        public object Items {
            get { return _items; }
        }
        private ObservableCollection<TableMapping> _items;
        #endregion  

        private TableMappingDetails TableMappingDetails { get; set; }
        
        #region SelectedItem
        public object SelectedItem {
            get { return _selectedItem; }
            set {
                _selectedItem = value;

                TableMapping mapping = _selectedItem as TableMapping;
                if (mapping != null && Model.TableToColumnMapping.ContainsKey(mapping)) {
                    ColumnMappingModel columnMappingModel = Model.TableToColumnMapping[mapping];
                    Model.SelectedTableMapping = mapping;
                    Model.SelectedColumnMappingModel = columnMappingModel;
                    TableMappingDetails.DataContext = Model;
                    if (!DatabaseManager.TestConnections(mapping.TableValidation.DbConfig, mapping.TableView.DbConfig)) {
                        TableMappingDetails.Visibility = Visibility.Collapsed;
                        MessageBox.Show(Owner, "Ein Fehler beim Öffnen der Datenbank-Verbindung ist aufgetreten:" + Environment.NewLine + DatabaseManager.LastError);
                    } else {
                        TableMappingDetails.Visibility = Visibility.Visible;
                        TableMappingDetails.Update();
                        DataPreviewModel dataPreviewModel = new DataPreviewModel(_selectedItem as TableMapping);
                        TableMappingDetails.SetDataPreviewModel(dataPreviewModel);
                    }
                    if((Owner as DlgMainWindow).Model != null) (Owner as DlgMainWindow).Model.SelectedTableMapping = _selectedItem as TableMapping;
                }

                OnPropertyChanged("SelectedItem");
                OnPropertyChanged("HeaderString");

            }
        }
        private object _selectedItem;
        #endregion

        private ProfileModel Model { get { return ProfileModelManager.GetModel(MainWindowModel.SelectedProfile); } }

        #endregion properties

        #region methods

        #region InitUiElements
        public void InitUiElements(System.Windows.Controls.Panel dataPanel, System.Windows.Controls.Panel listPanel) {
            this.DataPanel = dataPanel;
            this.ListPanel = listPanel;
            //listPanel.Children.Add(CreateListBox());
            listPanel.Children.Add(new CtlTableMappingDataGrid());
            //TableMappingDetails = new TableMappingDetails();
            TableMappingDetails = new TableMappingDetails(Owner);
            dataPanel.Children.Add(TableMappingDetails);
            TableMappingDetails.LostFocus += new RoutedEventHandler(TableMappingDetails_LostFocus);
        }

        void TableMappingDetails_LostFocus(object sender, RoutedEventArgs e) {
            (TableMappingDetails.DataContext as ProfileModel).SaveProfile();
        }

        #endregion

        #region HeaderString
        public string HeaderString {
            get {
                if (SelectedItem != null)
                    return ((TableMapping)SelectedItem).DisplayString;
                else
                    return "";
            }
        }
        #endregion

        #region AddItem
        public bool AddItem() {
            try {
                if (!InitTables()) {
                    return false;
                }
                TableMapping newTableMapping = new TableMapping();
                Model.TableMappingModel.Clear();
                DlgNewTableMapping dlgNewTableMapping = new DlgNewTableMapping(newTableMapping) { DataContext = Model.TableMappingModel, Owner = Owner };

                //Table mapping is added on success by the dialog
                if (dlgNewTableMapping.ShowDialog().Value) {
                    MainWindowModel.SelectedTableMapping = newTableMapping;
                    SelectedItem = newTableMapping;

                    Model.SaveProfile();
                    return true;
                }
                return false;
            } catch (Exception ex) {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        #region InitTables
        //private DbConfig _lastValidationConfig;
        private DbConfig _lastViewConfig;
        private bool InitTables() {
            if (Model.DatabaseModel.ConfigDest != null && string.IsNullOrEmpty(Model.DatabaseModel.ConfigDest.DbName)) {
                MessageBox.Show(Owner, "Bitte zuerst eine Datenbank für die View auswählen", "", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            //If the config has been changed since the last call, delete the current tables and retrieve the tables from the new database
            if (_lastViewConfig != null && (
                Model.DatabaseModel.ConfigDest.DbName != _lastViewConfig.DbName)) {
                    Model.TableMappingModel.ObsDestination.Clear();
                    Model.TableMappingModel.ObsSource.Clear();
            }

            if ((Model.TableMappingModel.ObsDestination.Count == 0 && Model.TableMappingModel.ObsSource.Count == 0) || Model.TableMappingModel.Mapping.Count == 0) {

                try {
                    _lastViewConfig = (DbConfig) Model.DatabaseModel.ConfigDest.Clone();

                    using (IDatabase view = ConnectionManager.CreateConnection(Model.DatabaseModel.ConfigDest)) {
                        view.Open();

                        Model.TableMappingModel.ObsDestination.Clear();
                        if (view.DatabaseExists(Model.DatabaseModel.ConfigDest.DbName + "_system")) {
                            IDataReader reader =
                                view.ExecuteReader("SELECT t.name FROM " + Model.DatabaseModel.ConfigDest.DbName +
                                                   "_system.table t where t.system_id = 2 order by t.name");
                            while (reader.Read())
                                Model.TableMappingModel.AddObsDestination(reader.GetValue(0).ToString());
                        }
                        else {
                            foreach (string table in view.GetTableList())
                                Model.TableMappingModel.AddObsDestination(table);
                        }
                    }
                } catch (Exception ex) {
                    MessageBox.Show(Owner, "Ein Fehler ist aufgetreten beim Öffnen der Datenbank Verbindung:" + Environment.NewLine + ex.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            return true;
        }
        #endregion InitTables
        
        #region DeleteSelectedItem
        public void DeleteSelectedItem() {
            DeleteItem(SelectedItem);
        }
        #endregion

        #region DeleteItem
        public void DeleteItem(object item) {
            if (item != null && item is TableMapping) {
                try {
                    if (MessageBox.Show(Owner, "Möchten Sie die Tabellenzuordnung " + (item as TableMapping).DisplayString +
                        " wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden.", "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                        == MessageBoxResult.Yes) {
                            Model.RemoveTableMapping(item as TableMapping);
                            MainWindowModel.RemovedTableMapping(item as TableMapping);
                            Model.SaveProfile();
                    }
                } catch (Exception ex) {
                    throw new Exception(ex.Message);
                }
            }
        }
        #endregion

        #endregion methods
        
    }
    
}
