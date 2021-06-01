using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AV.Log;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using TransDATA.Models;
using TransDATA.Models.ConfigModels;
using log4net;
using MessageBox = System.Windows.MessageBox;

namespace TransDATA.Controls.Config {
    /// <summary>
    /// Interaktionslogik für CtlCsvOutputConfig.xaml
    /// </summary>
    public partial class CtlCsvOutputConfig : INotifyPropertyChanged {
        internal ILog _log = LogHelper.GetLogger();

        public CtlCsvOutputConfig() {
            InitializeComponent();
            DataContextChanged += CtlCsvOutputConfig_DataContextChanged;
        }

        void CtlCsvOutputConfig_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            IProfile profile = DataContext as IProfile;
            if (profile != null)
                Model = new SelectCsvOutputModel(profile);
            else Model = null;    
        }

        private void btnCheckFileEncoding(object sender, RoutedEventArgs e) {
            try {
                Encoding enc = Encoding.GetEncoding(Model.OutputConfig.FileEncoding);
                MessageBox.Show(Base.Localisation.ResourcesCommon.ToolTipCheckFileEncodingOk,
                                Base.Localisation.ResourcesCommon.ToolTipCheckFileEncodingOk,
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            } catch (Exception ex) {
                using (NDC.Push(LogHelper.GetNamespaceContext()))
                {
                    _log.Error(ex.Message, ex);
                }

                MessageBox.Show(Base.Localisation.ResourcesCommon.ToolTipCheckFileEncodingError + System.Environment.NewLine + ex.Message,
                                Base.Localisation.ResourcesCommon.ToolTipCheckFileEncodingError,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }
        private void BtnSelectTargetFolderClick(object sender, RoutedEventArgs e) {
            var dlg = new FolderBrowserDialog { SelectedPath = Model.OutputConfig.Folder };
            if (dlg.ShowDialog() == DialogResult.OK)
                Model.OutputConfig.Folder = dlg.SelectedPath;
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion events

        #region Model
        private SelectCsvOutputModel _model;

        public SelectCsvOutputModel Model {
            get { return _model; }
            set {
                _model = value;
                OnPropertyChanged("Model");
            }
        }
        #endregion Model

        internal void UpdateUi(IProfile iProfile)
        {
            Model = new SelectCsvOutputModel(iProfile);
        }
    }
}
