// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using TransDATA.Models.ConfigModels;
using UserControl = System.Windows.Controls.UserControl;

namespace TransDATA.Controls.Config {
    /// <summary>
    /// Interaktionslogik für CtlGdpduConfig.xaml
    /// </summary>
    public partial class CtlGdpduOutputConfig : UserControl, INotifyPropertyChanged {
        public CtlGdpduOutputConfig() { InitializeComponent(); }

        private IGdpduConfig GdpduConfig { get { return DataContext as IGdpduConfig; } }

        private void BtnSelectTargetFolderClick(object sender, RoutedEventArgs e) {
            var dlg = new FolderBrowserDialog { SelectedPath = Model.OutputConfig.Folder };
            if (dlg.ShowDialog() == DialogResult.OK)
                Model.OutputConfig.Folder = dlg.SelectedPath;           
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) 
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion events

        #region Model
        private SelectGdpduOutputModel _model;

        public SelectGdpduOutputModel Model {
            get { return _model; }
            set {
                _model = value;
                OnPropertyChanged("Model");
            }
        }
        #endregion Model

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            IProfile profile = DataContext as IProfile;
            if (profile != null )
                Model = new SelectGdpduOutputModel(profile);
            else
                Model = null;    

        }

        internal void UpdateUi(IProfile iProfile)
        {
            Model = new SelectGdpduOutputModel(iProfile);
        }
    }
}