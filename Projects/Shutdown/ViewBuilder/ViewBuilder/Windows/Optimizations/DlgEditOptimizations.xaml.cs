using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using SystemDb.Compatibility.Viewbuilder.OptimizationRelated;
using AvdCommon.DataGridHelper;
using DbAccess;
using Utils;
using ViewBuilder.Models;
using ViewBuilderBusiness.Optimizations;

namespace ViewBuilder.Windows.Optimizations {
    /// <summary>
    /// Interaktionslogik für DlgEditOptimizations.xaml
    /// </summary>
    public partial class DlgEditOptimizations : Window {

        private PopupProgressBar _popupProgressBar;
        private ProgressCalculator _progressCalculator;
        private OptimizationsInstaller _optimizationsInstaller;
        private string _systemName = string.Empty;
        private string _error;

        public DlgEditOptimizations() {
            InitializeComponent();
        }

        EditOptimizationsModel Model { get { return DataContext as EditOptimizationsModel; } set { DataContext = value; } }

        private void btnManageLayers_Click(object sender, RoutedEventArgs e) {
            Model.ManageLayers();
        }

        private void tvOptimizations_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            Optimization opt = e.NewValue as Optimization;
            Model.SetupDataGrid(opt ?? e.OldValue as Optimization, dgEditTexts);
        }

        private void dgEditTexts_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            if ((e.Key >= Key.A && e.Key <= Key.Z) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || (e.Key >= Key.D0 && e.Key <= Key.D9))
                dgEditTexts.BeginEdit();
        }

        private void dgEditTexts_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e) {
            ContentPresenter cp = (ContentPresenter)e.EditingElement;

            TextBox destinationTextBox = VisualTreeHelper.GetChild(cp, 0) as TextBox;

            if (destinationTextBox != null) {
                destinationTextBox.Focus();
                destinationTextBox.SelectAll();
            }
            Model.IsInEditMode = true;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            Model.Save();
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            Close();
        }

        private void dgEditTexts_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (Model.IsInEditMode)
                return;
            if (e.Key == Key.Delete || e.Key == Key.Back) {
                Optimization deletedOpt = null;
                List<Optimization> opts = dgEditTexts.SelectedItems.Cast<Optimization>().ToList();

                foreach (Optimization opt in opts) {
                    if (opt != null) {
                        opt.Parent.Children.Remove(opt);
                        deletedOpt = opt;
                    }
                }
                if (deletedOpt != null) {
                    Optimization opt = deletedOpt.Parent.Children.FirstOrDefault();
                    if (opt != null)
                        opt.IsSelected = true;
                }
                e.Handled = true;
            }
        }

        private void miAddElement_Click(object sender, RoutedEventArgs e) {
            Optimization opt = ((MenuItem) sender).DataContext as Optimization;
            Model.AddOptimizationChild(opt);
            
        }

        private void btnAddFinancialYears_Click(object sender, RoutedEventArgs e) {
            Model.AddFinancialYears();
        }

        private void dgEditTexts_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) {
            Model.IsInEditMode = false;
        }

        private void btnReloadOptimizations_Click(object sender, RoutedEventArgs e) {
            MessageBoxResult result = MessageBox.Show(Properties.Resources.QuestionResetOptimizations, string.Empty, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result != MessageBoxResult.Yes) return;

            #region [ Initialize ProgressBar ]
            _progressCalculator = new ProgressCalculator { WorkerSupportsCancellation = true };
            _progressCalculator.DoWork += ProgressCalculatorOnDoWork;
            _progressCalculator.RunWorkerCompleted += ProgressCalculatorOnRunWorkerCompleted;
            _popupProgressBar = new PopupProgressBar()
            {
                DataContext = _progressCalculator,
                Owner = this
            };
            #endregion [ Initialize ProgressBar ]
            #region [ Initialize OptimizationsInstaller ]
            _optimizationsInstaller = new OptimizationsInstaller(Settings.CurrentProfileConfig);
            _optimizationsInstaller.Installing += OptimizationsInstallerOnInstalling;
            _optimizationsInstaller.InstallingComplete += OptimizationsInstallerOnInstallingComplete;
            _optimizationsInstaller.Clearing += OptimizationsInstallerOnClearing;
            _optimizationsInstaller.ClearingComplete += OptimizationsInstallerOnClearingComplete;
            _optimizationsInstaller.Initializing += OptimizationsInstallerOnInitializing;
            _optimizationsInstaller.InitializingComplete += OptimizationsInstallerOnInitializingComplete;
            _optimizationsInstaller.UpdatingMetadata += OptimizationsInstallerOnUpdatingMetadata;
            _optimizationsInstaller.UpdatingMetadataComplete += OptimizationsInstallerOnUpdatingMetadataComplete;
            _optimizationsInstaller.ErrorOccurred += OptimizationsInstallerOnErrorOccurred;
            #endregion [ Initialize OptimizationsInstaller ]

            _systemName = Model.Profile.DbConfig.DbName;
            _progressCalculator.RunWorkerAsync();
            _popupProgressBar.ShowDialog();
        }

        #region [ Helper methods ]

        private void OptimizationsInstallerOnErrorOccurred(object sender, ErrorEventArgs errorEventArgs)
        {
            _progressCalculator.CancelAsync();
            _error = string.Format(string.Format(Properties.Resources.ReinstallOptimizationsFailed, errorEventArgs.Message));
        }

        private void OptimizationsInstallerOnInitializingComplete(object sender, EventArgs eventArgs)
        {
            _progressCalculator.Description = "Initializing complete";
            _progressCalculator.StepDone();
        }

        private void OptimizationsInstallerOnInitializing(object sender, EventArgs eventArgs)
        {
            _progressCalculator.Description = "Initializing tables...";
        }

        private void OptimizationsInstallerOnClearingComplete(object sender, EventArgs eventArgs)
        {
            _progressCalculator.Description = "Clearing complete";
            _progressCalculator.StepDone();
        }

        private void OptimizationsInstallerOnClearing(object sender, EventArgs eventArgs)
        {
            _progressCalculator.Description = "Clearing tables...";
        }

        private void OptimizationsInstallerOnInstallingComplete(object sender, EventArgs eventArgs)
        {
            _progressCalculator.Description = "Installing complete";
        }

        private void OptimizationsInstallerOnInstalling(object sender, EventArgs eventArgs)
        {
            _progressCalculator.Description = "Installing...";
        }

        private void OptimizationsInstallerOnUpdatingMetadataComplete(object sender, EventArgs eventArgs)
        {
            _progressCalculator.Description = "Updating metadata complete";
        }

        private void OptimizationsInstallerOnUpdatingMetadata(object sender, EventArgs eventArgs)
        {
            _progressCalculator.Description = "Updating metadata...";
        }

        private void ProgressCalculatorOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs runWorkerCompletedEventArgs)
        {
            _progressCalculator.Description = "Reloading metadata...";
            if (string.IsNullOrWhiteSpace(_error)) {
                try {
                    using (IDatabase db = Model.Profile.ViewboxDb.ConnectionManager.GetConnection()) {
                        //Model.Profile.ViewboxDb.LoadOptimizations(db);
                        // DEVNOTE: languages are not loaded with the above, so the whole profile needs to be reinitialized
                        Model.Profile.ViewboxDb.LoadLanguages(db);
                        Model.Profile.ViewboxDb.LoadOptimizations(db);
                    }
                    DataContext = new EditOptimizationsModel(Model.Profile, this);
                    MessageBox.Show(Properties.Resources.ReinstallOptimizationsComplete, string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch(Exception ex) {
                    MessageBox.Show(string.Format(Properties.Resources.ReinstallOptimizationsFailed, ex.Message), string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else {
                MessageBox.Show(_error, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _popupProgressBar.Close();
        }

        private void ProgressCalculatorOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs) {
            _error = string.Empty;
            _progressCalculator.SetWorkSteps(2, false);
            _optimizationsInstaller.Install(_systemName);
        }

        #endregion [ Helper methods ]
    }
}
