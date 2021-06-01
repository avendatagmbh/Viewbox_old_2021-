// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-09-12
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Base.Localisation;
using Business;
using Config.Interfaces.Config;
using Config.Interfaces.DbStructure;
using TransDATA.Models;
using TransDATA.Models.ConfigModels;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlSelectDatabase.xaml
    /// </summary>
    public partial class CtlSelectDatabase : UserControl, INotifyPropertyChanged {
        public CtlSelectDatabase() {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(CtlSelectDatabase_DataContextChanged);
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion events

        void CtlSelectDatabase_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {

            //if (Model != null && !string.IsNullOrEmpty(Model.ConnectionStringBuilder.Dsn)) {
            //    optDSN.IsChecked = true;
            //}
            IProfile profile = DataContext as IProfile;
            if (profile != null && profile.InputConfig.Config is IDatabaseInputConfig) {
                Model = new SelectDatabaseInputModel(profile);
                optDSN.IsChecked = true;
            }
            //if (DataContext is IDatabaseInputConfig) {
            //    Model = new SelectDatabaseInputModel(DataContext as IDatabaseInputConfig);
            //    optDSN.IsChecked = true;
            //} else
            //    Model = null;
        }

        #region Model
        private SelectDatabaseInputModel _model;

        public SelectDatabaseInputModel Model {
            get { return _model; }
            set {
                _model = value;
                OnPropertyChanged("Model");
            }
        }
        #endregion

        //#region ConnectionString
        //private string _connectionString;

        //public string ConnectionString {
        //    get { return _connectionString; }
        //    set {
        //        _connectionString = value;
                
        //    }
        //}
        //#endregion

        private void btnTestConnection_Click(object sender, RoutedEventArgs e) {
            //System.Diagnostics.Debugger.Break();
            //Model.TestDbConnection(UIHelpers.TryFindParent<Window>(this));
            SelectDatabaseInputModel dc = (SelectDatabaseInputModel) DataContext;
            if(dc != null)dc.TestDbConnection(UIHelpers.TryFindParent<Window>(this));
        }
    }
}