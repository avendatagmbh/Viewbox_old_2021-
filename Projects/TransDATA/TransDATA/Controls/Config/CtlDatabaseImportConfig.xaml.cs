// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows;
using AV.Log;
using Config.Enums;
using Config.Interfaces.DbStructure;
using TransDATA.Models.ConfigModels;
using log4net;

namespace TransDATA.Controls.Config {
    /// <summary>
    /// Interaktionslogik für CtlDatabaseImportConfig.xaml
    /// </summary>
    public partial class CtlDatabaseImportConfig : INotifyPropertyChanged {
        internal ILog _log = LogHelper.GetLogger();


        public CtlDatabaseImportConfig() {
            InitializeComponent();
            DataContextChanged += CtlDatabaseImportConfigDataContextChanged;
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion events

        private SelectDatabaseInputModel Model { get { return DataContext as SelectDatabaseInputModel; } }

        private void CtlDatabaseImportConfigDataContextChanged(object sender,
                                                               DependencyPropertyChangedEventArgs
                                                                   dependencyPropertyChangedEventArgs) {
            if (DataContext is IProfile) {
                IProfile profile = DataContext as IProfile;
                if (profile.InputConfig.Type == InputConfigTypes.Database)
                    DataContext = new SelectDatabaseInputModel(DataContext as IProfile);
                else if (profile.InputConfig.Type == InputConfigTypes.Csv)
                    DataContext = new SelectCsvInputModel(DataContext as IProfile);
                else {
                    ArgumentOutOfRangeException ex=new ArgumentOutOfRangeException();
                    using (NDC.Push(LogHelper.GetNamespaceContext())) {
                        _log.Error(ex.Message, ex);
                    }

                    throw ex;
                }
                    
            }
        }

        public void UpdateUi(IProfile dataContext) { DataContext = new SelectDatabaseInputModel(dataContext); }
    }
}