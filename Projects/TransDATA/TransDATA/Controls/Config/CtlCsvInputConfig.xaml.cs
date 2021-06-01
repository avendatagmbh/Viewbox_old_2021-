using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Config.Config;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using TransDATA.Models;
using TransDATA.Models.ConfigModels;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace TransDATA.Controls.Config {
    /// <summary>
    /// Interaktionslogik für CtlCsvInputConfig.xaml
    /// </summary>
    public partial class CtlCsvInputConfig : UserControl {
        public CtlCsvInputConfig() {
            InitializeComponent();
            DataContextChanged += CtlCsvInputConfig_DataContextChanged;
        }

        void CtlCsvInputConfig_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (DataContext is IProfile) {
                DataContext = new SelectCsvInputModel(DataContext as IProfile);

                if (Model != null) Model.PropertyChanged += Model_PropertyChanged;
            }
        }

        void Model_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "CsvPreview") {
                AvdCommon.DataGridHelper.DataGridCreater.CreateColumns(dgPreview, Model.CsvPreview);
            }
        }

        private void BtnSelectTargetFolderClick(object sender, RoutedEventArgs e) {
            var dlg = new FolderBrowserDialog { SelectedPath = Model.InputConfig.Folder };
            if (dlg.ShowDialog() == DialogResult.OK)
                Model.InputConfig.Folder = dlg.SelectedPath;
        }

        private void BtnSelectTargetFolderLogClick(object sender, RoutedEventArgs e) {
            var dlg = new FolderBrowserDialog { SelectedPath = Model.InputConfig.FolderLog };
            if (dlg.ShowDialog() == DialogResult.OK)
                Model.InputConfig.FolderLog = dlg.SelectedPath;
        }

        private void btnCheckFileEncoding(object sender, RoutedEventArgs e) {
            if (Model != null) Model.DetectEncoding(UIHelpers.TryFindParent<Window>(this));
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion events

        #region Model

        public SelectCsvInputModel Model
        {
            get { return DataContext as SelectCsvInputModel; ; }
        }
        #endregion Model

        internal void UpdateUi(IProfile iProfile)
        {
            DataContext = new SelectCsvInputModel(iProfile);
            if (Model != null) Model.PropertyChanged += Model_PropertyChanged;
        }

        private void BtnDetectLineEndClick(object sender, RoutedEventArgs e) {
            if (Model != null) Model.DetectLineEnd(UIHelpers.TryFindParent<Window>(this));
        }
    }
}
