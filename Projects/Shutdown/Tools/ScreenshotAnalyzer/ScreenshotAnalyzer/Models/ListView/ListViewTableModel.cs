using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using ScreenshotAnalyzer.Controls.ProfileRelated;
using ScreenshotAnalyzer.Manager;
using ScreenshotAnalyzer.Models.ProfileRelated;
using ScreenshotAnalyzer.Resources.Localisation;
using ScreenshotAnalyzer.Windows;
using ScreenshotAnalyzerBusiness.Manager;
using ScreenshotAnalyzerBusiness.Structures.Config;

namespace ScreenshotAnalyzer.Models.ListView {
    public class ListViewTableModel : ListViewModelBase, IListViewModel, INotifyPropertyChanged {
        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion

        public ListViewTableModel(MainWindowModel mainWindowModel) {
            MainWindowModel = mainWindowModel;
            mainWindowModel.PropertyChanged += MainWindowModel_PropertyChanged;
        }

        void MainWindowModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "SelectedProfile") {
                _items = MainWindowModel.SelectedProfile == null ? null : MainWindowModel.SelectedProfile.Tables;
                OnPropertyChanged("Items");
            }
            if (e.PropertyName == "SelectedTable") {
                TableDetails.DataContext = new TableModel(MainWindowModel.SelectedTable);
                if (SelectedItem != null) {
                    MainWindowModel.SelectedTable.PropertyChanged -= Table_PropertyChanged;
                    MainWindowModel.SelectedTable.PropertyChanged += Table_PropertyChanged;
                }

                OnPropertyChanged("HeaderString");
                OnPropertyChanged("SelectedItem");
            }
        }

        #region properties
        #region Items
        public object Items {
            get { return _items; }
        }
        private ObservableCollection<Table> _items;
        #endregion

        private MainWindowModel MainWindowModel { get; set; }

        private CtlTableDetails TableDetails { get; set; }

        #region SelectedItem
        public object SelectedItem {
            //get { return _selectedItem; }
            get { return MainWindowModel.SelectedTable; }
            set {
                MainWindowModel.SelectedTable = (Table) value;
            }
        }

        void Table_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            OnPropertyChanged("HeaderString");
        }
        //private object _selectedItem;
        #endregion

        #endregion properties

        #region methods

        #region InitUiElements
        public void InitUiElements(System.Windows.Controls.Panel dataPanel, System.Windows.Controls.Panel listPanel) {
            this.DataPanel = dataPanel;
            this.ListPanel = listPanel;
            listPanel.Children.Add(CreateListBox());

            TableDetails = new CtlTableDetails(){DataContext=null};
            dataPanel.Children.Add(TableDetails);
        }

        #endregion

        #region HeaderString
        public string HeaderString {
            get {
                if (SelectedItem != null)
                    return ((Table)SelectedItem).DisplayString;
                else
                    return "";
            }
        }
        #endregion

        #region AddItem
        public bool AddItem() {
            if (MainWindowModel.SelectedProfile != null) {
                SelectedItem = MainWindowModel.SelectedProfile.AddTable();
            }
            else {
                MessageBox.Show(MainWindowModel.DlgMainWindow, ResourcesGui.ListViewTableModel_AddItem_ErrorSelectProfileFirst);
            }
            return false;
        }
        #endregion

        #region DeleteSelectedItem
        public void DeleteSelectedItem() {
            DeleteItem(SelectedItem);
        }
        #endregion

        #region DeleteItem
        public void DeleteItem(object item) {
            if (item != null && item is Table) {
                try {
                    if (MessageBox.Show(Owner, string.Format(ResourcesGui.ListViewTableModel_DeleteItem_ConfirmationMessage, (item as Table).Name)
                        , "", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
                        == MessageBoxResult.Yes) {
                        MainWindowModel.SelectedProfile.RemoveTable(item as Table);
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
