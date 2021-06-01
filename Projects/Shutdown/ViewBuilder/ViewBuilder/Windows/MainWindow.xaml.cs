/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-11      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AV.Log;
using AvdWpfControls.Utils;
using DbAccess;
using ViewBuilder.Converters;
using ViewBuilder.Windows.Optimizations;
using ViewBuilderBusiness;
using ViewBuilderBusiness.EventArgs;
using System.IO;
using ViewBuilderBusiness.Structures.Config;
using ViewBuilder.Models;
using AvdWpfControls;
using ViewBuilderBusiness.Manager;
using ProjectDb.Tables;
using Utils;
using log4net;
using LogManager = AvdCommon.Logging.LogManager;
using ViewBuilderBusiness.Reports;
using DBComparisonBusiness.Business;
using ViewBuilder.Helpers;

namespace ViewBuilder.Windows
{

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PopupProgressBar _progressBar;
        private ILog log = LogHelper.GetLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow(ProfileConfig profile)
        {
            InitializeComponent();

            lblVersion.Content = AssemblyHelper.GetAssemblyVersion();
            
            // Get the build date           
            lblAssemblyDate.Content = string.Format("Version date: {0}", AssemblyHelper.GetAssemblyDate());
            

            _model = new MainWindowModel(this) { Profile = profile };

            // init viewbuilder
            _model.ViewBuilder.Error += viewBuilder_Error;
            _model.ViewBuilder.WorkerStarted += viewBuilder_WorkerStarted;
            _model.ViewBuilder.WorkerFinished += viewBuilder_WorkerFinished;

            _model.PropertyChanged += _model_PropertyChanged;
            this.DataContext = _model;
            chkIsViewCheckedHeader.DataContext = _model;

            if (_model.Profile != null)
            {
                _model.Profile.PropertyChanged += Profile_PropertyChanged;
                _model.Profile.Error += Profile_Error;

                if (_model.Profile.ConnectionManager != null)
                {
                    _model.Profile.ConnectionManager.PropertyChanged += ConnectionManager_PropertyChanged;
                }

                if (_model.Profile.ProjectDb != null)
                {
                    _model.Profile.ProjectDb.PropertyChanged += ProjectDb_PropertyChanged;
                    _model.Profile.ProjectDb.Error += ProjectDb_Error;

                    if (_model.Profile.ProjectDb.ConnectionManager != null)
                    {
                        _model.Profile.ProjectDb.ConnectionManager.PropertyChanged += ConnectionManagerPrj_PropertyChanged;
                    }
                }

                if (_model.Profile.SysDb != null)
                {
                    //_model.Profile.SysDb.PropertyChanged +=new System.ComponentModel.PropertyChangedEventHandler(SysDb_PropertyChanged);
                    //_model.Profile.SysDb.Error += new EventHandler<MessageEventArgs>(SysDb_Error);

                    _model.Profile.SysDb.PropertyChanged += SysDb_PropertyChanged;
                    _model.Profile.SysDb.Error += SysDb_Error;

                    if (_model.Profile.SysDb.ConnectionManager != null)
                    {
                        _model.Profile.SysDb.ConnectionManager.PropertyChanged += ConnectionManagerSys_PropertyChanged;
                    }
                }

                if (_model.Profile.ViewboxDb != null)
                {
                    //_model.Profile.SysDb.PropertyChanged +=new System.ComponentModel.PropertyChangedEventHandler(SysDb_PropertyChanged);
                    //_model.Profile.SysDb.Error += new EventHandler<MessageEventArgs>(SysDb_Error);

                    _model.Profile.ViewboxDb.PropertyChanged += ViewBoxDb_PropertyChanged;
                    _model.Profile.ViewboxDb.Error += ViewBoxDb_Error;

                    if (_model.Profile.ViewboxDb.ConnectionManager != null)
                    {
                        _model.Profile.ViewboxDb.ConnectionManager.PropertyChanged += ConnectionManagerViewbox_PropertyChanged;
                    }
                }

                if (_model.Profile.IndexDb != null)
                {
                    _model.Profile.IndexDb.PropertyChanged += IndexDb_PropertyChanged;

                    if (_model.Profile.IndexDb.IndexDbConnection != null)
                    {
                        _model.Profile.IndexDb.IndexDbConnection.PropertyChanged += ConnectionManagerIndex_PropertyChanged;
                    }
                }
            }
        }

        private static ConnectionStateToSignalStateConverter _connectionStateToSignalStateConverter = new ConnectionStateToSignalStateConverter();
        private delegate void voidDelegate();

        /*****************************************************************************************************/

        #region fields

        MainWindowModel _model;

        #endregion fields

        /*****************************************************************************************************/

        #region eventHandler

        private void btnCreateViewboxDb_Click(object sender, RoutedEventArgs e)
        {
            _model.CreateViewboxDb(this);
        }

        void ProjectDb_Error(object sender, Utils.ErrorEventArgs e)
        {
            log.ErrorWithCheck(e.Message);
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            MessageBox.Show(this, e.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
        }

        ///// <summary>
        ///// Handles the Error event of the SysDb control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="Utilities.EventsArgs.MessageEventArgs"/> instance containing the event data.</param>
        //void SysDb_Error(object sender, Utils.ErrorEventArgs e) {
        //    log.ErrorWithCheck(e.Message);
        //    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
        //                delegate
        //                {
        //                    MessageBox.Show(this, e.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        //                }));
        //}

        /// <summary>
        /// Handles the Error event of the Profile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Utilities.EventsArgs.MessageEventArgs"/> instance containing the event data.</param>
        void Profile_Error(object sender, Utils.ErrorEventArgs e)
        {
            log.ErrorWithCheck(e.Message);
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            MessageBox.Show(this, e.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
        }

        void ViewBoxDb_Error(object sender, System.IO.ErrorEventArgs e)
        {
            string msg = e.GetException().Message;
            log.ErrorWithCheck(msg);
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            MessageBox.Show(this, msg, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
        }

        void SysDb_Error(object sender, System.IO.ErrorEventArgs e)
        {
            string msg = e.GetException().Message;
            log.ErrorWithCheck(msg);
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            MessageBox.Show(this, msg, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                        }));
        }

        /// <summary>
        /// Handles the PropertyChanged event of the _model control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Profile":
                    _model.Profile.PropertyChanged += Profile_PropertyChanged;
                    _model.Profile.Error += Profile_Error;
                    break;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Profile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void Profile_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConnectionManager":
                    if (_model.Profile.ConnectionManager != null)
                        _model.Profile.ConnectionManager.PropertyChanged += ConnectionManager_PropertyChanged;
                    break;

                case "SysDb":
                    _model.Profile.SysDb.PropertyChanged += SysDb_PropertyChanged;
                    //_model.Profile.SysDb.ConnectionManager.PropertyChanged += ConnectionManagerSys_PropertyChanged;
                    _model.Profile.SysDb.SystemDbInitialized += SysDb_SystemDbInitialized;
                    break;

                case "ViewboxDb":
                    _model.Profile.ViewboxDb.PropertyChanged += ViewBoxDb_PropertyChanged;
                    //_model.Profile.ViewboxDb.ConnectionManager.PropertyChanged += ConnectionManagerViewbox_PropertyChanged;
                    _model.Profile.ViewboxDb.SystemDbInitialized += ViewboxDbInitialized;
                    _model.Profile.ViewboxDb.Error += ViewboxDb_Error;
                    break;

                case "ProjectDb":
                    _model.Profile.ProjectDb.PropertyChanged += ProjectDb_PropertyChanged;
                    _model.Profile.ProjectDb.Error += ProjectDb_Error;
                    break;
                case "IndexDb":
                    _model.Profile.IndexDb.PropertyChanged += IndexDb_PropertyChanged;
                    break;

                case "IsInitialized":
                    if (_model.Profile.IsInitialized)
                    {
                        UpdateViewscripts();
                    }
                    break;
            }
        }

        void ViewboxDb_Error(object sender, System.IO.ErrorEventArgs e)
        {
            Exception ex = e.GetException();
            string msg = "Fehler beim initialisieren der Viewbox Datenbank: " + Environment.NewLine + e.GetException().Message;
            log.Error(msg, ex);
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() => MessageBox.Show(this, msg, "", MessageBoxButton.OK, MessageBoxImage.Error)));
        }

        void SysDb_SystemDbInitialized(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            sysDbSignal.State = (SignalLightStates)_connectionStateToSignalStateConverter.Convert(
                                _model.Profile.SysDb.ConnectionManager.ConnectionState,
                                typeof(SignalLightStates),
                                null,
                                System.Globalization.CultureInfo.CurrentCulture);
                            sysDbSignal.ToolTip = _model.Profile.SysDb.ConnectionManager.DbConfig.DbName;
                        }));
        }

        void ViewboxDbInitialized(object sender, EventArgs e) {
            bool isUpToDate = true;
            SystemDb.SystemDb viewboxDb = (sender as SystemDb.SystemDb);
            using (IDatabase db = viewboxDb.ConnectionManager.GetConnection())
            {
                if (!db.DatabaseExists(_model.Profile.ViewboxDbName) || !db.TableExists("users"))
                {
                    _model.CreateViewboxDb(this);
                }
                else
                {
                    var databaseOutOfDateInformation = viewboxDb.HasDatabaseUpgrade(db);
                    if (databaseOutOfDateInformation != null)
                    {
                        //MessageBox mit Info wegen Update der ViewboxDb 
                        this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            if (MessageBox.Show(this,
                                 Properties.Resources.UpgradeMsg,
                                 Properties.Resources.UpgradeDatabase,
                                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                            {
                                viewboxDb.UpgradeDatabase(db);
                                viewboxDb.LoadTables(db, false);
                            }
                            else {
                                isUpToDate = false;
                            }
                        }));
                    }
                    else if (_model.Profile.ViewboxDb.Objects.All(o => o.Database.ToLower() != _model.Profile.DbConfig.DbName.ToLower()))
                        _model.CreateViewboxDb(this);
                }
            }

            //if (!isUpToDate) _model.Profile.ViewboxDb.ConnectionManager = null;
            if (!isUpToDate) {
                _model.Profile.ViewboxDb.ConnectionManager.Dispose();
                _model.ViewBuilder.IsFailedToLoad = true;
            } else {
                _model.ViewBuilder.IsFailedToLoad = false;
            }

            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            viewboxDbSignal.State = (SignalLightStates)_connectionStateToSignalStateConverter.Convert(
                                _model.Profile.ViewboxDb.ConnectionManager.ConnectionState,
                                typeof(SignalLightStates),
                                null,
                                System.Globalization.CultureInfo.CurrentCulture);
                            viewboxDbSignal.ToolTip = _model.Profile.ViewboxDb.ConnectionManager.DbConfig.DbName;
                            _model.ViewBuilder.IsReady = viewboxDbSignal.State == SignalLightStates.Green;
                        }));
        }

        /// <summary>
        /// Handles the PropertyChanged event of the ProjectDb control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void ProjectDb_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConnectionManager":
                    if (_model.Profile.ProjectDb.ConnectionManager != null)
                        _model.Profile.ProjectDb.ConnectionManager.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(ConnectionManagerPrj_PropertyChanged);
                    break;
            }
        }

        void SysDb_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConnectionManager":
                    if (_model.Profile.SysDb.ConnectionManager != null)
                        _model.Profile.SysDb.ConnectionManager.PropertyChanged +=
                        new System.ComponentModel.PropertyChangedEventHandler(ConnectionManagerSys_PropertyChanged);
                    break;
            }
        }

        void ViewBoxDb_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConnectionManager":
                    if (_model.Profile.ViewboxDb.ConnectionManager != null)
                        _model.Profile.ViewboxDb.ConnectionManager.PropertyChanged +=
                        new System.ComponentModel.PropertyChangedEventHandler(ConnectionManagerViewbox_PropertyChanged);
                    break;
            }
        }


        void IndexDb_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Db":
                    if (_model.Profile.IndexDb.IndexDbConnection != null)
                        _model.Profile.IndexDb.IndexDbConnection.PropertyChanged += ConnectionManagerIndex_PropertyChanged;
                    break;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the ConnectionManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void ConnectionManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConnectionState":
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            dataDbSignal.State = (SignalLightStates)_connectionStateToSignalStateConverter.Convert(
                                _model.Profile.ConnectionManager.ConnectionState,
                                typeof(SignalLightStates),
                                null,
                                System.Globalization.CultureInfo.CurrentCulture);
                            dataDbSignal.ToolTip = _model.Profile.ConnectionManager.DbConfig.DbName;
                        }));
                    break;
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the ConnectionManagerSys control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void ConnectionManagerSys_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConnectionState":
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            sysDbSignal.State = (SignalLightStates)_connectionStateToSignalStateConverter.Convert(
                                _model.Profile.SysDb.ConnectionManager.ConnectionState,
                                typeof(SignalLightStates),
                                null,
                                System.Globalization.CultureInfo.CurrentCulture);
                            sysDbSignal.ToolTip = _model.Profile.SysDb.ConnectionManager.DbConfig.DbName;
                        }));
                    break;
            }
        }

        void ConnectionManagerViewbox_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConnectionState":
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            viewboxDbSignal.State = (SignalLightStates)_connectionStateToSignalStateConverter.Convert(
                                //_model.Profile.SysDb.ConnectionManager.ConnectionState,
                                _model.Profile.ViewboxDb.ConnectionManager.ConnectionState,
                                typeof(SignalLightStates),
                                null,
                                System.Globalization.CultureInfo.CurrentCulture);
                            viewboxDbSignal.ToolTip = _model.Profile.ViewboxDb.ConnectionManager.DbConfig.DbName;
                            _model.ViewBuilder.IsReady = viewboxDbSignal.State == SignalLightStates.Green;
                        }));
                    break;
            }
        }

        void ConnectionManagerIndex_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConnectionState":
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            if (_model.Profile.IndexDb.IndexDbConnection.ConnectionState == DbAccess.ConnectionStates.Online)
                            {
                                _model.IndexCalculator = new ProgressCalculator();
                                _model.IndexCalculator.Title = "Checking indexdb";
                                _model.IndexCalculator.DoWork += progress_DoCheckIndexData;
                                _model.IndexCalculator.WorkerSupportsCancellation = true;
                                _model.IndexCalculator.RunWorkerCompleted += progress_DoCheckIndexData_RunWorkerCompleted;
                                _model.IsCheckingIndexDb = true;
                                _model.IndexCalculator.RunWorkerAsync();
                                /*_model.PopupProgressBar = new PopupProgressBar() { Owner = this, DataContext = progress };
                                _model.PopupProgressBar.ShowDialog();*/
                            }
                            else
                            {
                                indexDbSignal.State =
                                    (SignalLightStates) _connectionStateToSignalStateConverter.Convert(
                                        _model.Profile.IndexDb.IndexDbConnection.ConnectionState,
                                        typeof (SignalLightStates),
                                        null,
                                        System.Globalization.CultureInfo.CurrentCulture);
                                indexDbSignal.PopupContent = "";
                            }
                        }));
                    break;
            }
        }

        private string _hasIndexErrors = "";

        private void progress_DoCheckIndexData(object sender, DoWorkEventArgs e)
        {
            ProgressCalculator progress = sender as ProgressCalculator;
            progress.Description = "Checking index data...";

            _hasIndexErrors = _model.Profile.IndexDb.TestReferencedObjectsExists(_model.Profile.ConnectionManager.DbConfig.DbType, progress);
            if (progress.CancellationPending)
                e.Cancel = true;

            progress.Description = "Checking index data finished...";
            _model.IsCheckingIndexDb = false;
        }

        private void progress_DoCheckIndexData_RunWorkerCompleted(object sender,
                                                                 RunWorkerCompletedEventArgs e)
        {
            if (!String.IsNullOrEmpty(_hasIndexErrors))
            {
                log.Info(
                    "There are some warnings in the Index database. Check the log files, or re-run the index db creation." +
                    _hasIndexErrors);
                indexDbSignal.State = (SignalLightStates)_connectionStateToSignalStateConverter.Convert(
                    DbAccess.ConnectionStates.Connecting,
                    typeof(SignalLightStates),
                    null,
                    System.Globalization.CultureInfo.CurrentCulture);
                indexDbSignal.PopupContent = _hasIndexErrors;
                indexDbSignal.ExtMethod = new voidDelegate(() => _model.Profile.IndexDb.Init(_model.Profile.ViewboxDb, _model.Profile.MaxWorkerThreads.Value));
            }
            else
            {

                indexDbSignal.State = (SignalLightStates) _connectionStateToSignalStateConverter.Convert(
                    _model.Profile.IndexDb.IndexDbConnection.ConnectionState,
                    typeof (SignalLightStates),
                    null,
                    System.Globalization.CultureInfo.CurrentCulture);
                indexDbSignal.PopupContent = "";
            }
            _model.IsCheckingIndexDb = false;
        }


        /// <summary>
        /// Handles the PropertyChanged event of the ConnectionManagerSys control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        void ConnectionManagerPrj_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ConnectionState":
                    this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                        delegate
                        {
                            prjDbSignal.State = (SignalLightStates)_connectionStateToSignalStateConverter.Convert(
                                _model.Profile.ProjectDb.ConnectionManager.ConnectionState,
                                typeof(SignalLightStates),
                                null,
                                System.Globalization.CultureInfo.CurrentCulture);
                            prjDbSignal.ToolTip = _model.Profile.ProjectDb.ConnectionManager.DbConfig.DbName;
                        }));
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnQuit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the btnCreateViews control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCreateViews_Click(object sender, RoutedEventArgs e)
        {
            if (_model.UpdateViewscripts(true))
                new System.Threading.Thread(new ParameterizedThreadStart(_model.ViewBuilder.CreateViews)).Start(new ViewCreateOptions() { GenerateDistinctDataOnly = false });
        }

        /// <summary>
        /// Handles the Click event of the btnCreateIndexForViews control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCreateIndexForViews_Click(object sender, RoutedEventArgs e)
        {
            if (_model.UpdateViewscripts(true))
                new System.Threading.Thread(new ParameterizedThreadStart(_model.ViewBuilder.CreateViews)).Start(new ViewCreateOptions() { GenerateDistinctDataOnly = true });
        }

        /// <summary>
        /// Handles the WorkerStarted event of the viewBuilder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void viewBuilder_WorkerStarted(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new EventHandler(viewBuilder_WorkerStarted), sender, new object[] { e });
                return;
            }

            _model.Workers.Add((WorkerState)sender);
        }

        /// <summary>
        /// Handles the WorkerFinished event of the viewBuilder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void viewBuilder_WorkerFinished(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new EventHandler(viewBuilder_WorkerFinished), sender, new object[] { e });
                return;
            }

            _model.Workers.Remove((WorkerState)sender);
        }

        /// <summary>
        /// Handles the Error event of the viewBuilder control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void viewBuilder_Error(object sender, EventArgs e)
        {
            if (e is Utils.ErrorEventArgs)
            {
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(
                    delegate
                    {
                        MessageBox.Show(this,
                            ((Utils.ErrorEventArgs)e).Message,
                            "Fehler",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }));

                System.Diagnostics.Debug.WriteLine(((Utils.ErrorEventArgs)e).Message);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCance
        /// l control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Einpielvorgang abbrechen?",
                "",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {

                _model.ViewBuilder.Cancel();
            }
        }

        /// <summary>
        /// Handles the Ok event of the config dialog controls..
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void dlgConfig_Ok(object sender, EventArgs e)
        {
            try
            {
                // reload profile to update database connections
                _model.Profile = ViewBuilderBusiness.Manager.ProfileManager.Open(_model.Profile.Name);

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Ok event of the dlgSelectScriptSource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void dlgSelectScriptSource_Ok(object sender, EventArgs e)
        {
            try
            {
                ProfileManager.Save(_model.Profile);
                UpdateViewscripts();

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRefresScripts control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnRefresScripts_Click(object sender, RoutedEventArgs e)
        {
            UpdateViewscripts();
        }

        /// <summary>
        /// Handles the Click event of the btnEditProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnEditProfile_Click(object sender, RoutedEventArgs e)
        {
            DlgEditProfile DlgEditProfile = new DlgEditProfile(_model.Profile);
            DlgEditProfile.Owner = this;
            DlgEditProfile.Ok += new EventHandler(dlgConfig_Ok);
            DlgEditProfile.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the btnSelectScriptSource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectScriptSource_Click(object sender, RoutedEventArgs e)
        {
            DlgScriptSource dlgSelectScriptSource = new DlgScriptSource(_model.Profile.ScriptSource);
            dlgSelectScriptSource.Owner = this;
            dlgSelectScriptSource.Ok += new EventHandler(dlgSelectScriptSource_Ok);
            dlgSelectScriptSource.ShowDialog();
        }

        /// <summary>
        /// Handles the Click event of the btnConfigMail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnConfigMail_Click(object sender, RoutedEventArgs e)
        {
            DlgConfigMail dlgConfigMail = new DlgConfigMail(_model.Profile.Mail);
            dlgConfigMail.Owner = this;
            dlgConfigMail.Ok += new EventHandler(dlgConfig_Ok);
            dlgConfigMail.ShowDialog();
        }

        /// <summary>
        /// Handles the Checked event of the chkIsViewCheckedHeader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void chkIsViewCheckedHeader_Checked(object sender, RoutedEventArgs e)
        {
            if (_model.Profile == null) return;
            foreach (Viewscript view in _model.Profile.Viewscripts)
            {
                view.IsChecked = true;
            }

            // update persistated state
            if (_model.Profile.Viewscripts.Count > 0)
            {
                _model.Profile.Viewscripts[0].CheckAll();
            }
        }

        /// <summary>
        /// Handles the Unchecked event of the chkIsViewCheckedHeader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void chkIsViewCheckedHeader_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_model.Profile == null) return;
            foreach (Viewscript view in _model.Profile.Viewscripts)
            {
                // DEVNOTE: quick and dirty fix for the case if there is null enrty among viewscripts
                if (view != null) view.IsChecked = false;
            }

            // update persistated state
            if (_model.Profile.Viewscripts.Count > 0)
            {
                _model.Profile.Viewscripts[0].UncheckAll();
            }
        }

        /// <summary>
        /// Handles the Click event of the chkIsChecked control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void chkIsChecked_Click(object sender, RoutedEventArgs e)
        {
            chkIsViewCheckedHeader.IsChecked = null;
            DataGridRow row = FindVisualParent<DataGridRow>((CheckBox)sender);
            ((Viewscript)row.DataContext).SaveOrUpdate();
        }

        /// <summary>
        /// Handles the PreviewMouseLeftButtonDown event of the DataGridCell control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            if (cell != null && !cell.IsEditing && !cell.IsReadOnly)
            {
                if (!cell.IsFocused)
                {
                    cell.Focus();
                }
                DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                if (dataGrid != null)
                {
                    if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                    {
                        if (!cell.IsSelected)
                            cell.IsSelected = true;
                    }
                    else
                    {
                        DataGridRow row = FindVisualParent<DataGridRow>(cell);
                        if (row != null && !row.IsSelected)
                        {
                            row.IsSelected = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelectProfile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnSelectProfile_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                new PopupProfileSelection(_model)
                {
                    Owner = this,
                    Left = _lastClickedPos.X + 3,
                    Top = _lastClickedPos.Y + 20
                }.ShowDialog();
            }
            catch (Exception) { }
        }

        private Point _lastClickedPos;
        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _lastClickedPos = e.GetPosition(this);
        }

        /// <summary>
        /// Handles the Click event of the btnCreateTmpViewDb control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void btnCreateTmpViewDb_Click(object sender, RoutedEventArgs e)
        {
            _model.CreateTempViewDb();
        }

        /// <summary>
        /// Handles the Closing event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_model.ViewBuilder.IsRunning)
            {
                e.Cancel = true;
                MessageBox.Show
                    (this, "Das Programm kann derzeit nicht beendet werden, da noch eine Vieweinspielung läuft.",
                    "Beenden",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Handles the PreviewMouseKeyDown event of the DataGridCell control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void DataGridCell_PreviewMouseKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                DataGridCell cell = (DataGridCell)sender;
                chkIsViewCheckedHeader.IsChecked = null;

                DataGridRow row = FindVisualParent<DataGridRow>(cell);
                Viewscript view = (Viewscript)(row.DataContext);
                view.IsChecked = !view.IsChecked;
                view.SaveOrUpdate();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the MouseEnter event of the Border control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void Border_MouseEnter(object sender, MouseEventArgs e)
        {
            ListBoxItem lb = FindVisualParent<ListBoxItem>((Border)sender);
            WorkerState ws = (WorkerState)lb.Content;
            if (ws.JobInfo != null && ws.JobInfo is Viewscript)
            {
                _model.HoveredView = (Viewscript)ws.JobInfo;
                _model.HoveredView.IsHovered = true;
            }
        }

        /// <summary>
        /// Handles the MouseLeave event of the Border control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void Border_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_model.HoveredView != null)
            {
                _model.HoveredView.IsHovered = false;
                _model.HoveredView = null;
            }
        }

        /// <summary>
        /// Handles the MouseMove event of the dgViewscriptCell control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void dgViewscriptCell_MouseMove(object sender, MouseEventArgs e)
        {

            if (_model.PopupViewscriptDetails == null) return;

            Point p = e.GetPosition(this);
            p.X += this.Left;
            p.Y += this.Top;

            if (p.X + _model.PopupViewscriptDetails.Width > SystemParameters.VirtualScreenWidth - 50)
            {
                p.X -= _model.PopupViewscriptDetails.Width;
            }
            else
            {
                p.X += 20;
            }

            if (p.Y + _model.PopupViewscriptDetails.Height > SystemParameters.VirtualScreenHeight - 50)
            {
                p.Y -= _model.PopupViewscriptDetails.Height - 30;
            }
            else
            {
                p.Y += 40;
            }

            _model.PopupViewscriptDetails.Left = p.X;
            _model.PopupViewscriptDetails.Top = p.Y;
            _model.PopupViewscriptDetails.MaxHeight = Math.Max(100, Math.Min(SystemParameters.VirtualScreenHeight - p.Y, 500));
            this.Focus();
        }

        /// <summary>
        /// Handles the MouseEnter event of the dgViewscriptCell control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void dgViewscriptCell_MouseEnter(object sender, MouseEventArgs e)
        {

            DataGridCell cell = (DataGridCell)sender;
            if (cell.Column.Equals(dgScriptsCol_IsChecked)) return;

            _model.PopupViewscriptDetails = new PopupViewscriptDetails { Owner = this, DataContext = ((ContentPresenter)cell.Content).Content };

            Point p = e.GetPosition(this);
            p.X += this.Left;
            p.Y += this.Top;

            if (p.X + _model.PopupViewscriptDetails.Width > SystemParameters.VirtualScreenWidth - 50)
            {
                p.X -= _model.PopupViewscriptDetails.Width;
            }
            else
            {
                p.X += 20;
            }

            if (p.Y + _model.PopupViewscriptDetails.Height > SystemParameters.VirtualScreenHeight - 50)
            {
                p.Y -= _model.PopupViewscriptDetails.Height - 30;
            }
            else
            {
                p.Y += 40;
            }

            _model.PopupViewscriptDetails.Left = p.X;
            _model.PopupViewscriptDetails.Top = p.Y;
        }

        /// <summary>
        /// Handles the MouseLeave event of the dgViewscriptCell control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void dgViewscriptCell_MouseLeave(object sender, MouseEventArgs e)
        {

            if (_model.PopupViewscriptDetails == null) return;

            // should happen that window is in closing state we we try to close again
            try {
                _model.PopupViewscriptDetails.Close();
                _model.PopupViewscriptDetails = null;
            }
            catch {
            }
        }

        #endregion eventHandler

        /*****************************************************************************************************/

        #region methods

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        private void Disconnect()
        {
            //new Login().Show();
            this.Close();
        }

        /// <summary>
        /// Finds the visual parent.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }

        /// <summary>
        /// Handles the Closed event of the Window control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Window_Closed(object sender, EventArgs e)
        {
            _model.Dispose();
        }

        /// <summary>
        /// Updates the viewscripts.
        /// </summary>
        private void UpdateViewscripts()
        {
            
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                                                                                                     {
                                                                                                         dgViewscripts.ItemsSource= null;
                                                                                                         _model.UpdateViewscripts(false);
                                                                                                         dgViewscripts.ItemsSource = _model.Profile.Viewscripts;
                                                                                                     }));
            
        }

        #endregion methods

        private void btnDeleteView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show(this, "Wollen Sie den ausgewählten View wirklich löschen?", "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                    return;
                _model.DeleteView();
                MessageBox.Show(this, "View erfolgreich gelöscht.", "",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Fehler beim Löschen des Views: " + Environment.NewLine + ex.Message, "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCreateLog_Click(object sender, RoutedEventArgs e)
        {
            _model.CreateLog(this);
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            ImageButton element = sender as ImageButton;

            new DlgViewscriptDetails() { Owner = this, DataContext = element.DataContext }.ShowDialog();
        }

        private void btnUpdateMetadata_Click(object sender, RoutedEventArgs e)
        {
            _model.UpdateMetadata();
        }

        private void btnUpdateAllMetadata_Click(object sender, RoutedEventArgs e)
        {
            _model.UpdateAllMetadata(_model.Profile.Views);
        }

        private void btnAddFakes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _model.AddFakes(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Fehler beim Hinzufügen der Basisdaten:" + Environment.NewLine + ex.Message, "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddDeleteEntries_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _model.DeleteAndAddEntry();
                MessageBox.Show(this, "Metadaten erfolgreich aktualisiert.", "",
                                MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Fehler beim Aktualisieren der Metadaten: " + Environment.NewLine + ex.Message, "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void btnStartViewbox_Click(object sender, RoutedEventArgs e)
        {
            _model.StartViewbox();
        }

        private void btnEditOptimizations_Click(object sender, RoutedEventArgs e)
        {
            _model.EditOptimizations();
        }

        private void btnAddRelations_Click(object sender, RoutedEventArgs e)
        {
            _model.AddRelations();
        }

        private void btnCheckDatatypes_Click(object sender, RoutedEventArgs e)
        {
            _model.CheckDatatypes();
        }

        private void btnReorderTables_Click(object sender, RoutedEventArgs e)
        {
            _model.ReorderTables();
        }

        private void btnTemp_Click(object sender, RoutedEventArgs e)
        {
            _model.FindViewsWithTables();
        }

        private void btnCreateIndex_Click(object sender, RoutedEventArgs e)
        {
            _model.CreateIndexData();
            _model.Profile.IndexDb.Init(_model.Profile.ViewboxDb, _model.Profile.MaxWorkerThreads.Value);
        }

        private void btnError_Click(object sender, RoutedEventArgs e)
        {
            ImageButton element = sender as ImageButton;

            new DlgViewError((element.DataContext as View).Error).ShowDialog();
        }

        private void btnWarning_Click(object sender, RoutedEventArgs e)
        {
            ImageButton element = sender as ImageButton;

            new DlgViewError((element.DataContext as View).Warnings).ShowDialog();
        }

        /// <summary>
        /// Copies the value of the datagrid to the clipping board
        /// !!! switch should be updated if binding path changes in datagrid's textblocks.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DgViews_OnCopyingRowClipboardContent(object sender, DataGridRowClipboardEventArgs e)
        {
            string rowValues = "";
            var view = (e.ClipboardRowContent.First().Item as View);
            if (view == null) return;
            e.ClipboardRowContent.Clear();
            foreach (var column in (sender as DataGrid).Columns)
            {
                string cellValue = "";
                switch (column.Header.ToString())
                {
                    case "Name":
                        cellValue = view.Name; break;
                    case "Beschreibung":
                        cellValue = view.Description; break;
                    case "Bearbeiter":
                        cellValue = view.Agent; break;
                    case "Zeitstempel":
                        cellValue = view.CreationTimestampDisplayString; break;
                    case "Dauer":
                        cellValue = view.DurationDisplayString; break;
                }
                e.ClipboardRowContent.Add(new DataGridClipboardCellContent(e.Item, column, cellValue));
            }
        }
        private void btnGeneratePDFReport_Click(object sender, RoutedEventArgs e)
        {
            _model.GeneratePDFReport();
        }

        private void btnUpdateLanguageMetadata_Click(object sender, RoutedEventArgs e) {
            _model.UpdateLanguageMetaData();
        }

        private void btnGenerateEmptyDistinct_Click(object sender, RoutedEventArgs e)
        {
            _model.GenerateEmptyDistinct();
        }

        private void btnSapBillSchemaImport_Click(object sender, RoutedEventArgs e)
        {
            if(_model.SapBillSchemaImportEnabled) {
                DlgSapBillSchemaImport dlg = new DlgSapBillSchemaImport();
                dlg.ViewModel.FilePath = _model.Profile.ScriptSource.BilanzDirectory;
                dlg.Owner = this;
                dlg.Ok += DlgSapBillSchemaImport_Ok;
                dlg.ShowDialog();
            }else {
                MessageBox.Show(this, "For this function you must create the required script or T0011t table is missing. " + Environment.NewLine + "Example: sap_balance, blg_bilanz_guv", "Missing script", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        void DlgSapBillSchemaImport_Ok(object sender, EventArgs e)
        {
            DlgSapBillSchemaImport dlg = (DlgSapBillSchemaImport) sender;

            try
            {
                _model.SapBillSchemaImportFn(dlg.ViewModel.Files);
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

		private void btnCheckDifferences_Click(object sender, RoutedEventArgs e)
		{
			_model.CheckDatabaseDifferences();
		}

        private void btnOpenTableArchivingSetupDialog_Click(object sender, RoutedEventArgs e)
        {
            DlgArchivingTableSelector m_Dialog = new DlgArchivingTableSelector(_model.Profile);
            m_Dialog.ShowDialog();
        }

        private void btnParseScripts_Click(object sender, RoutedEventArgs e)
        {
            ProgressCalculator progress = new ProgressCalculator();
            progress.Title = "Parsing viewscripts";
            progress.DoWork += _model.ParseViewScripts;
            progress.RunWorkerCompleted += UpdateViewScriptsCompleted;
            progress.RunWorkerAsync();
            _progressBar = new PopupProgressBar() { Owner = this };
            _progressBar.DataContext = progress;
            _progressBar.ShowDialog();
        }

        void UpdateViewScriptsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _progressBar.Close();
        }

        private void btnParsingErrorInfo_Click(object sender, RoutedEventArgs e)
        {
            ImageButton element = sender as ImageButton;

            new DlgViewError((element.DataContext as Viewscript).ParsingError).ShowDialog();
        }

        private void btnGetExtendedColumnInformations_Click(object sender, RoutedEventArgs e)
        {
            _model.GetExtendedColumnInformations();
        }

    }
}
