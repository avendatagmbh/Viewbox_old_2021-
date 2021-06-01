// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-13
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using System.Windows;
using Config.Interfaces.DbStructure;
using TransDATA.Models.ConfigModels;

namespace TransDATA.Controls.Config {
    /// <summary>
    /// Interaktionslogik für CtlDatabaseExportConfig.xaml
    /// </summary>
    public partial class CtlDatabaseExportConfig : INotifyPropertyChanged {
        public CtlDatabaseExportConfig() {
            InitializeComponent();
            DataContextChanged += CtlDatabaseExportConfigDataContextChanged;
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion events

        private SelectDatabaseOutputModel Model { get { return DataContext as SelectDatabaseOutputModel; } }

        private void CtlDatabaseExportConfigDataContextChanged(object sender,
                                                               DependencyPropertyChangedEventArgs
                                                                   dependencyPropertyChangedEventArgs) {
            //IDatabaseOutputConfig config = DataContext as IDatabaseOutputConfig;
            if (DataContext is IProfile) {
                DataContext = new SelectDatabaseOutputModel(DataContext as IProfile);
                lstParameters.ItemsSource = ((SelectDatabaseOutputModel) DataContext).ConnectionStringBuilder.Params;
            }
        }

        public void UpdateUi(IProfile dataContext) {
            DataContext = new SelectDatabaseOutputModel(dataContext);
            lstParameters.ItemsSource = ((SelectDatabaseOutputModel) DataContext).ConnectionStringBuilder.Params;
        }

        private void BtnTestConnectionClick(object sender, RoutedEventArgs e) { Model.TestDbConnection(UIHelpers.TryFindParent<Window>(this)); }
    }
}