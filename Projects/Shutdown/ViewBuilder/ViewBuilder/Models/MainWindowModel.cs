/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2010-10-23      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using AV.Log;
using DbAccess;
using Utils;
using ViewBuilder.Models.MetadataUpdate;
using ViewBuilder.Structures;
using ViewBuilderBusiness.SapBillSchemaImport;
using ViewBuilder.Windows.MetadataUpdate;
using ViewBuilderBusiness.Structures;
using ViewBuilderBusiness.Structures.Config;
using System.Collections.ObjectModel;
using ViewBuilderBusiness.EventArgs;
using ViewBuilder.Windows;
using ProjectDb.Tables;
using ViewBuilderBusiness.Structures.Pdf;
using log4net;
using DlgEditOptimizations = ViewBuilder.Windows.Optimizations.DlgEditOptimizations;
using ViewBuilderBusiness.Reports;
using ViewBuilderBusiness.MetadataUpdate;
using System.Threading;
using MessageBox = System.Windows.MessageBox;
using View = ProjectDb.Tables.View;
using ViewBuilder.Properties;
using System.Linq;
using DBComparisonBusiness.Business;
using System.Xml.Linq;

namespace ViewBuilder.Models
{

    /// <summary>
    /// Model for the MainWindow window.
    /// </summary>
    internal class MainWindowModel : INotifyPropertyChanged, IDisposable
    {

        private ILog log = LogHelper.GetLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowModel"/> class.
        /// </summary>
        public MainWindowModel(Window mainWindow)
        {
            this.Workers = new ObservableCollection<WorkerState>();
            this.IsCheckedHeaderState = null;
            this.ViewBuilder = new ViewBuilderBusiness.ViewBuilder();
            _mainWindow = mainWindow;
        }

        /*****************************************************************************************************/

        #region events

        /// <summary>
        /// Occurs when a property changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion events

        /*****************************************************************************************************/

        #region eventTrigger

        /// <summary>
        /// Called when a property changed.
        /// </summary>
        /// <param name="property">The property.</param>
        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion eventTrigger

        /*****************************************************************************************************/

        #region fields

        /// <summary>
        /// See property Profile.
        /// </summary>
        ProfileConfig _profile;

        /// <summary>
        /// See property PopupViewscriptDetails.
        /// </summary>
        PopupViewscriptDetails _popupViewscriptDetails;
        private Window _mainWindow;
        private View _selectedView;

        #endregion fields

        /*****************************************************************************************************/

        #region properties

        /// <summary>
        /// Gets or sets the profile.
        /// </summary>
        /// <value>The profile.</value>
        public ProfileConfig Profile
        {
            get { return _profile; }
            set
            {
                if (_profile != value)
                {
                    if (_profile != null)
                    {
                        //ProfileManager.Save(_profile);
                        _profile.Dispose();
                        value.AssignedUser = _profile.AssignedUser;
                    }

                    _profile = value;
                    _profile.Viewscripts.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Views_CollectionChanged);

                    if (!value.IsInitialized)
                    {
                        var mw = _mainWindow as MainWindow;
                        if (mw != null)
                        {
                            mw.dgViewscripts.ItemsSource = null;
                            value.Init();
                        }
                        else
                            value.Init();
                    }

                    ViewBuilder.Profile = _profile;

                    OnPropertyChanged("Profile");
                }
            }
        }


        /// <summary>
        /// Handles the CollectionChanged event of the Views control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        void Views_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // check if new views has been added
            if (e.NewItems != null) IsCheckedHeaderState = null;
        }

        /// <summary>
        /// Gets or sets the state of the is checked header.
        /// </summary>
        /// <value>The state of the is checked header.</value>
        public bool? IsCheckedHeaderState { get; set; }

        /// <summary>
        /// Gets or sets the workers.
        /// </summary>
        /// <value>The workers.</value>
        public ObservableCollection<WorkerState> Workers { get; private set; }

        /// <summary>
        /// Gets or sets the view builder.
        /// </summary>
        /// <value>The view builder.</value>
        public ViewBuilderBusiness.ViewBuilder ViewBuilder { get; private set; }

        /// <summary>
        /// Gets or sets the popup viewscript details.
        /// </summary>
        /// <value>The popup viewscript details.</value>
        public PopupViewscriptDetails PopupViewscriptDetails
        {
            get { return _popupViewscriptDetails; }
            set
            {
                if (_popupViewscriptDetails != value)
                {

                    if ((value != null) && (_popupViewscriptDetails != null))
                    {
                        _popupViewscriptDetails.Close();
                    }

                    _popupViewscriptDetails = value;
                }

            }
        }

        /// <summary>
        /// Gets or sets the hovered view.
        /// </summary>
        /// <value>The hovered view.</value>
        public Viewscript HoveredView { get; set; }

        public Viewscript SelectedViewscript { get; set; }

        public View SelectedView
        {
            get { return _selectedView; }
            set
            {
                if (_selectedView != value)
                {
                    _selectedView = value;
                    OnPropertyChanged("SelectedView");
                }
            }
        }

        private ProgressCalculator _indexCalculator;
        public ProgressCalculator IndexCalculator
        {
            get { return _indexCalculator; }
            set { _indexCalculator = value; OnPropertyChanged("IndexCalculator"); }
        }

        private bool _isCheckingIndexDb;
        public bool IsCheckingIndexDb
        {
            get { return _isCheckingIndexDb; }
            set { _isCheckingIndexDb = value; OnPropertyChanged("IsCheckingIndexDb"); }
        }

        #endregion Properties

        #region Methods
        public void Dispose()
        {
            // cancel viewbuilder process, if actually running
            ViewBuilder.Cancel();

            // dispose profile (disposes also the system-db interface and the connection managers for both databases)
            Profile.Dispose();

            // close popup window
            if (PopupViewscriptDetails != null)
            {
                PopupViewscriptDetails.Close();
            }

        }

        public void CreateViewboxDb(MainWindow mainWindow)
        {
            mainWindow.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(
                        delegate
                        {
                            DlgCreateViewboxDb dlgCreateViewboxDb = new DlgCreateViewboxDb()
                            {
                                Owner = mainWindow,
                                DataContext = new CreateViewboxDbModel(Profile)
                            };
                            dlgCreateViewboxDb.ShowDialog();
                        }));
        }

        public bool UpdateViewscripts(bool updateOnlyChecked)
        {
            ViewscriptUpdater updater = new ViewscriptUpdater(_mainWindow, Profile);
            updater.UpdateViewscripts(updateOnlyChecked);
            return !updater.HadError;
        }

        public void DeleteView()
        {
            if (SelectedView == null) return;
            ViewBuilder.DeleteView(SelectedView);
        }


        #region CreateLog
        public void CreateLog(MainWindow parent)
        {
            LogOptionsModel model = new LogOptionsModel();
            DlgLogOptions dialog = new DlgLogOptions() { Owner = parent, DataContext = model };
            bool? dialogResult = dialog.ShowDialog();
            if (dialogResult != null && dialogResult.Value)
            {
                PdfLogWriter pdfWriter = new PdfLogWriter(model.FileName);
                pdfWriter.WritePdf();
            }
        }

        #endregion CreateLog


        public void UpdateMetadata()
        {
            if (SelectedView == null) throw new Exception("Kein View ausgewählt.");
            UpdateAllMetadata(new List<View>() { SelectedView });
        }

        #region UpdateAllMetadata
        private PopupProgressBar _progressBar;
        public void UpdateAllMetadata(IEnumerable<View> views)
        {
            ProgressCalculator progress = new ProgressCalculator();
            progress.WorkerSupportsCancellation = true;
            progress.Title = "Aktualisiere Metadaten";
            progress.DoWork += progress_DoWork;
            progress.RunWorkerCompleted += progress_RunWorkerCompleted;
            progress.RunWorkerAsync(views);
            _progressBar = new PopupProgressBar() { DataContext = progress, Owner = _mainWindow };
            _progressBar.ShowDialog();
            //ViewBuilder.UpdateAllMetadata(Profile.Views);
        }

        void progress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _mainWindow.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Error == null)
                    MessageBox.Show(_mainWindow,
                                "Metadaten erfolgreich aktualisiert.", "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                _progressBar.Close();
            }));
        }

        void progress_DoWork(object sender, DoWorkEventArgs e)
        {
            ProgressCalculator progress = sender as ProgressCalculator;
            IEnumerable<View> views = e.Argument as IEnumerable<View>;
            try
            {
                ViewBuilder.UpdateAllMetadata(views, progress);
            }
            catch (Exception ex)
            {
                _mainWindow.Dispatcher.BeginInvoke(new Action(() => MessageBox.Show(_mainWindow,
                                                                                    "Fehler beim Aktualisieren der Metadaten: " +
                                                                                    Environment.NewLine + ex.Message, "",
                                                                                    MessageBoxButton.OK, MessageBoxImage.Error)));

            }
        }
        #endregion UpdateAllMetadata

        public void AddFakes(Window parent)
        {
            ManageFakesModel model = new ManageFakesModel(Profile.ViewboxDb, Profile.DbConfig.DbName);
            DlgManageFakes dialog = new DlgManageFakes() { Owner = parent, DataContext = model };
            dialog.ShowDialog();
        }

        public void DeleteAndAddEntry()
        {
            ViewBuilder.DeleteAndAddEntries(Profile.Viewscripts);
        }

        #region StartViewbox
        public void StartViewbox()
        {
            try
            {
                StartViewboxImpl();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow,
                                "Beim Starten der Viewbox ist ein unerwarteter Fehler aufgetreten: " + ex.Message);
            }
        }

        public void CheckDatatypes()
        {
            try
            {
                Profile.ViewboxDb.CheckDatatypes();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow,
                                "Beim Analysieren der Datentypen ist ein Fehler aufgetreten: " + ex.Message);
            }
        }

        public void ReorderTables()
        {
            try
            {
                Profile.ViewboxDb.ReorderTables();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow,
                                "Beim Aufbauen der Order-Area ist ein Fehler aufgetreten: " + ex.Message);
            }
        }

        private void StartViewboxImpl()
        {
            //string path1 = @"C:\Program Files (x86)\Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer40.EXE";
            //string path2 = @"C:\Program Files\Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer40.EXE";
            if (!File.Exists(ViewboxStarter.Path1) && !File.Exists(ViewboxStarter.Path2))
            {
                MessageBox.Show(_mainWindow,
                                "Der Web Development Server steht nicht zur Verfügung, da die Datei " + ViewboxStarter.Path1 +
                                Environment.NewLine + " und " + ViewboxStarter.Path2 + " nicht existieren.");
                return;
            }

            //string viewboxSrc = @"C:\temp\viewbox_src";
            //if (!Directory.Exists(viewboxSrc)) {
            //    Directory.CreateDirectory(viewboxSrc);
            ViewboxStarter viewboxStarter = new ViewboxStarter(Profile.ViewboxDb.ConnectionManager.DbConfig);

            ProgressCalculator copyDataProgress = viewboxStarter.CreateProgressCalculator();
            copyDataProgress.RunWorkerCompleted += copyDataProgress_RunWorkerCompleted;
            _progressBar = new PopupProgressBar() { DataContext = copyDataProgress, Owner = _mainWindow };
            viewboxStarter.StartViewbox();
            _progressBar.ShowDialog();
            if (!Convert.ToBoolean(copyDataProgress.UserState))
            {
                return;
            }
            else
            {
                MessageBox.Show(_mainWindow, "Viewbox erfolgreich gestartet");
            }
            //}
        }
        void copyDataProgress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                _mainWindow.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        _progressBar.Close();
                        MessageBox.Show(_mainWindow, "Fehler beim Kopieren: " + e.Error.Message);
                    }));
            }
            else
            {
                _mainWindow.Dispatcher.BeginInvoke(new Action(() => _progressBar.Close()));
            }
        }

        #endregion StartViewbox

        #region EditOptimizations
        public void EditOptimizations()
        {
            try
            {
                DlgEditOptimizations dlg = new DlgEditOptimizations()
                {
                    DataContext = new EditOptimizationsModel(Profile, _mainWindow),
                    Owner = _mainWindow
                };
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow, "Es ist ein unerwarteter Fehler aufgetreten: " + ex.Message, "",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion EditOptimizations

        #region CreateTempViewDb
        private PopupProgressBar _popupProgress;
        public PopupProgressBar PopupProgressBar
        {
            get { return _popupProgress; }
            internal set { _popupProgress = value; }
        }

        public void CreateTempViewDb()
        {
            try
            {
                ProgressCalculator progress = new ProgressCalculator();
                progress.Title = "Erstelle TmpViewDb";
                progress.DoWork += progress_DoCreateTmpViewDb;
                progress.WorkerSupportsCancellation = true;
                progress.RunWorkerCompleted += progressCreateTempViewDb_RunWorkerCompleted;
                progress.RunWorkerAsync();
                _popupProgress = new PopupProgressBar() { Owner = _mainWindow, DataContext = progress };
                _popupProgress.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow, "Fehler beim Erstellen der TmpViewDb: " + ex.Message, "Fehler",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void progressCreateTempViewDb_RunWorkerCompleted(object sender,
                                                                 RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(_mainWindow,
                                "Fehler: " + e.Error.Message,
                                "Fehler",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                MessageBox.Show(_mainWindow,
                                "Prozess abgebrochen",
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            else
                MessageBox.Show(_mainWindow,
                                "Tempviewdatenbank erfolgreich erstellt",
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

            _popupProgress.Close();
        }

        private void progress_DoCreateTmpViewDb(object sender, DoWorkEventArgs e)
        {
            ProgressCalculator progress = sender as ProgressCalculator;
            using (var db = Profile.ConnectionManager.GetConnection())
            {
                List<string> tables = new List<string>();

                progress.Description = "Initialisiere Datenbank...";

                tables = db.GetTableList();

                progress.SetWorkSteps(tables.Count + 1, false);

                progress.StepDone();

                string tmpViewDbName = db.DbConfig.DbName + ViewBuilderBusiness.ViewBuilder.TmpViewDbNameBase;
                tmpViewDbName = tmpViewDbName.Substring(0, tmpViewDbName.Length - 1);

                //Delete existing TmpViewDb
                db.DropDatabaseIfExists(tmpViewDbName);

                //Create new TmpViewDb
                db.CreateDatabaseIfNotExists(tmpViewDbName);

                //Create Database-Views for all tables in list
                ViewBuilder.CreateTempViewTable(new WorkerState(0), db, tmpViewDbName, tables, progress);
                if (progress.CancellationPending)
                    e.Cancel = true;
                progress.Description = "TmpViewDb-Erstellung beendet...";
            }
        }

        public void AddRelations()
        {
            RelationsModel model = new RelationsModel(_profile);
            DlgRelations dlg = new DlgRelations() { Owner = _mainWindow, DataContext = model };
            model.Owner = dlg;
            dlg.ShowDialog();
        }

        #endregion CreateTempViewDb


        #region CreateIndexData

        public void CreateIndexData()
        {
            try
            {
                bool recreateIfExists =
                    MessageBox.Show(_mainWindow,
                                    "Would you like to recreate index data for all parameters, or just for the non processed ones?", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) ==
                    MessageBoxResult.Yes;
                ProgressCalculator progress = new ProgressCalculator();
                progress.Title = "Creating indexdb";
                progress.DoWork += progress_DoCreateIndexData;
                progress.WorkerSupportsCancellation = true;
                progress.ExtInfo = recreateIfExists;
                progress.RunWorkerCompleted += progress_DoCreateIndexData_RunWorkerCompleted;
                progress.RunWorkerAsync();
                _popupProgress = new PopupProgressBar() { Owner = _mainWindow, DataContext = progress };
                _popupProgress.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow, "Creating index data - error: " + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void progress_DoCreateIndexData(object sender, DoWorkEventArgs e)
        {
            ProgressCalculator progress = sender as ProgressCalculator;
            progress.Description = "Creating index data...";
            bool recreateIfExists = progress.ExtInfo == null || (bool)progress.ExtInfo;
            var result = IndexDb.IndexDb.DoJob(Profile.ViewboxDb, progress, Profile.MaxWorkerThreads.Value, 0, 0, recreateIfExists);
            if (result != "")
                MessageBox.Show("Error while creating index data:" + result);
            if (progress.CancellationPending)
                e.Cancel = true;

            progress.Description = "Creating index data finished...";
        }

        private void progress_DoCreateIndexData_RunWorkerCompleted(object sender,
                                                                 RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(_mainWindow,
                                "Creating index data - error: " + e.Error.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                MessageBox.Show(_mainWindow,
                                "Creating index data - progress canceled",
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            else
                MessageBox.Show(_mainWindow,
                                "Creating index data successfull",
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

            _popupProgress.Close();
        }

        #endregion CreateIndexData

        #endregion Methods

        public void FindViewsWithTables()
        {
            CsvReader reader = new CsvReader("C:\\temp\\dbdupont_dupont_archive_3.csv");
            reader.Separator = ';';
            reader.HeadlineInFirstRow = false;
            var csv = reader.GetCsvData(100, Encoding.Default);
            foreach (var viewscript in Profile.Viewscripts)
            {
                var tables = viewscript.GetTables();
                var lowerTables = new List<string>();
                foreach (var table in tables)
                    lowerTables.Add(table.Key.ToLower());
                foreach (DataRow csvRow in csv.Rows)
                {
                    if (lowerTables.Contains(csvRow.ItemArray[0].ToString().ToLower()))
                    {
                        Console.WriteLine(viewscript.Name + " - " + viewscript.Description + ", by " + csvRow[0].ToString());
                        break;
                    }
                }
            }
        }
        internal void GeneratePDFReport()
        {
            try
            {
                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

                dialog.Description = "Please select a folder location";
                dialog.ShowNewFolderButton = true;
                string folderLocation = "";

                //#if DEBUG
                //                folderLocation = Environment.GetEnvironmentVariable("USERPROFILE");
                //#else
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    folderLocation = dialog.SelectedPath;

                    //#endif
                    var reportDetailDialog = new DlgReportDetailsPrompt();
                    var canceled = false;
                    reportDetailDialog.CancelClicked += () =>
                                                            {
                                                                canceled = true;
                                                            };
                    if (reportDetailDialog.ShowDialog() == true)
                    {

                        if (canceled) return;

                        var projectManager = reportDetailDialog.ProjectManager;
                        var projectNumber = reportDetailDialog.ProjectNumber;

                        ProgressCalculator progress = new ProgressCalculator();
                        progress.Title = "Erstelle PDF Report";

                        #region progress.DoWork

                        progress.DoWork += (sender, e) =>
                                               {
                                                   ProgressCalculator pc = sender as ProgressCalculator;
                                                   new PDFReportGenerator(
                                                       Path.Combine(folderLocation, "viewsReport.pdf"),
                                                       Profile.ViewboxDb, projectManager, projectNumber).
                                                       CreateAndSaveDocument(progress);

                                               };

                        #endregion

                        progress.WorkerSupportsCancellation = true;

                        #region progress.RunWorkerCompleted

                        progress.RunWorkerCompleted += (sender, e) =>
                                                           {

                                                               if (e.Error != null)
                                                               {
                                                                   MessageBox.Show(_mainWindow,
                                                                                   "Fehler: " + e.Error.Message,
                                                                                   "Fehler",
                                                                                   MessageBoxButton.OK,
                                                                                   MessageBoxImage.Error);
                                                               }
                                                               else if (e.Cancelled)
                                                               {
                                                                   MessageBox.Show(_mainWindow,
                                                                                   "Prozess abgebrochen",
                                                                                   "",
                                                                                   MessageBoxButton.OK,
                                                                                   MessageBoxImage.Information);
                                                               }
                                                               else
                                                                   MessageBox.Show(_mainWindow,
                                                                                   "PDF Report erfolgreich erstellt",
                                                                                   "",
                                                                                   MessageBoxButton.OK,
                                                                                   MessageBoxImage.Information);

                                                               _popupProgress.Close();
                                                           };

                        #endregion

                        progress.RunWorkerAsync();
                        _popupProgress = new PopupProgressBar() { Owner = _mainWindow, DataContext = progress };
                        _popupProgress.ShowDialog();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow, "Fehler beim Erstellen der PDF Report: " + ex.Message, "Fehler",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal void UpdateLanguageMetaData()
        {
            try
            {
                ProgressCalculator progress = new ProgressCalculator();
                progress.Title = "Updating language metadata";
                progress.DoWork += progress_DoUpdateLanguageMetaData;
                progress.WorkerSupportsCancellation = true;
                progress.RunWorkerCompleted += progress_DoUpdateLanguageMetaData_RunWorkerCompleted;
                progress.RunWorkerAsync();
                _popupProgress = new PopupProgressBar() { Owner = _mainWindow, DataContext = progress };
                _popupProgress.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow, "Updating language metadata - error: " + ex.Message, "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void progress_DoUpdateLanguageMetaData(object sender, DoWorkEventArgs e)
        {
            using (IDatabase conn = Profile.ViewboxDb.ConnectionManager.GetConnection())
            {
                try
                {
                    conn.BeginTransaction();
                    ProgressCalculator progress = sender as ProgressCalculator;
                    progress.Description = "Updating languages...";
                    ViewBuilderBusiness.MetadataUpdate.UpdateViewBoxMetadata.UpdateLanguages(Profile, conn, progress);
                    if (progress.CancellationPending)
                        e.Cancel = true;
                    else
                    {
                        progress.Description = "Updating property texts metadata...";
                        ViewBuilderBusiness.MetadataUpdate.UpdateViewBoxMetadata.UpdatePropertyTexts(Profile, conn, progress);
                    }
                    if (progress.CancellationPending)
                        e.Cancel = true;
                    else
                    {
                        progress.Description = "Updating optimization texts metadata...";
                        ViewBuilderBusiness.MetadataUpdate.UpdateViewBoxMetadata.UpdateOptimizationTexts(Profile, conn, progress);
                        progress.Description = "Updating category texts metadata...";
                        ViewBuilderBusiness.MetadataUpdate.UpdateViewBoxMetadata.UpdateCategoryTexts(Profile, conn, progress);
                    }
                    if (progress.CancellationPending)
                        e.Cancel = true;
                    else
                    {
                        progress.Description = "Updating issue and parameter metadata...";
                        ViewBuilderBusiness.MetadataUpdate.UpdateViewBoxMetadata.UpdateIssueAndParameterLanguageTexts(Profile, conn, progress);
                    }
                    if (progress.CancellationPending)
                        e.Cancel = true;
                    else
                    {
                        progress.Description = "Updating table/column metadata...";
                        ViewBuilderBusiness.MetadataUpdate.UpdateSapMetadata.UpdateDescriptions(Profile, conn, progress);
                    }
                    progress.Description = "Updating language metadata finished...";

                    if (progress.CancellationPending)
                        e.Cancel = true;
                    else
                    {
                        progress.Description = "Object type metadata...";
                        ViewBuilderBusiness.MetadataUpdate.UpdateViewBoxMetadata.UpdateObjectTypesTexts(Profile, conn, progress);
                    }
                    progress.Description = "Object type metadata finished...";
                    conn.CommitTransaction();
                }
                catch (Exception)
                {
                    if (conn.HasTransaction()) conn.RollbackTransaction();
                    throw;
                }
            }
        }

        private void progress_DoUpdateLanguageMetaData_RunWorkerCompleted(object sender,
                                                                 RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(_mainWindow,
                                "Updating language metadata - error: " + e.Error.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                MessageBox.Show(_mainWindow,
                                "Updating language metadata - canceled",
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            else
                MessageBox.Show(_mainWindow,
                                "Updating language metadata successfull",
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

            _popupProgress.Close();
        }

        internal void GenerateEmptyDistinct()
        {
            try
            {
                MessageBoxResult result = MessageBox.Show(_mainWindow,
                                "This process can take long hours (up to 1-2 days)." + Environment.NewLine +
                                "Do you want to continue?", "Generating empty/distinct metadata", MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    ProgressCalculator progress = new ProgressCalculator();
                    progress.Title = "Generating empty/distinct metadata";
                    progress.DoWork += progress_DoGenerateEmptyDistinctMetaData;
                    progress.WorkerSupportsCancellation = true;
                    progress.RunWorkerCompleted += progress_DoGenerateEmptyDistinctMetaData_RunWorkerCompleted;
                    progress.RunWorkerAsync();
                    _popupProgress = new PopupProgressBar() { Owner = _mainWindow, DataContext = progress };
                    _popupProgress.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow, "Generating empty/distinct metadata - error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void progress_DoGenerateEmptyDistinctMetaData(object sender, DoWorkEventArgs e)
        {
            ProgressCalculator progress = sender as ProgressCalculator;

            progress.Description = "Generating empty/distinct metadata...";

            Profile.ViewboxDb.CreateEmptyDistinctTable(progress);

            progress.Description = "Generating empty/distinct metadata finished...";
        }

        private void progress_DoGenerateEmptyDistinctMetaData_RunWorkerCompleted(object sender,
                                                                 RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(_mainWindow,
                                "Generating empty/distinct metadata - error: " + e.Error.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                MessageBox.Show(_mainWindow,
                                "Generating empty/distinct metadata - canceled",
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            else
                MessageBox.Show(_mainWindow,
                                "Generating empty/distinct metadata successfull",
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

            _popupProgress.Close();
        }

        internal void SapBillSchemaImportFn(IList<SapBillSchemaImportFile> files)
        {
            try
            {

                ProgressCalculator progress = new ProgressCalculator();
                progress.Title = "Sap Bill Schema Import";

                progress.DoWork += progress_SapBillSchemaImport;
                progress.WorkerSupportsCancellation = true;
                progress.RunWorkerCompleted += progress_SapBillSchemaImport_RunWorkerCompleted;
                progress.RunWorkerAsync(files);
                _popupProgress = new PopupProgressBar() { Owner = _mainWindow, DataContext = progress };
                _popupProgress.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow, "Generating empty/distinct metadata - error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void progress_SapBillSchemaImport(object sender, DoWorkEventArgs e)
        {
            ProgressCalculator progress = sender as ProgressCalculator;
            IList<SapBillSchemaImportFile> files = (IList<SapBillSchemaImportFile>)e.Argument;
            SapBillSchemaImport imprtr = new SapBillSchemaImport(files, Profile, progress);
            imprtr.DoWork2();
        }

        private void progress_SapBillSchemaImport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(_mainWindow,
                                "Sap Bill Schema Import - error: " + e.Error.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                MessageBox.Show(_mainWindow,
                                "Sap Bill Schema Import - canceled",
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            else
                MessageBox.Show(_mainWindow,
                                "Sap Bill Schema Import successfull",
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

            _popupProgress.Close();
        }



        public bool SapBillSchemaImportEnabled
        {
            get { return SapBillSchemaImport.FunctionEnabled(Profile); ; }
        }

        internal void CheckDatabaseDifferences()
        {
            try
            {
                var checkConsistencyModel = new CheckConsistencyModel(_profile, _mainWindow);
                checkConsistencyModel.CheckConsistency();
            }
            catch (Exception ex)
            {
                MessageBox.Show(_mainWindow, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Parse ViewScripts

        /// <summary>
        /// Parses the Viewscripts and saves the parsing errors and missing tables, fields to database
        /// </summary>
        public void ParseViewScripts(object sender, DoWorkEventArgs e)
        {
            ProgressCalculator progress = sender as ProgressCalculator;

            using (var connection = Profile.ConnectionManager.GetConnection())
            {
                string[] viewscripts = Profile.Viewscripts.Where(vs => vs.IsChecked).Select(vs => vs.FileName).ToArray();
                progress.SetWorkSteps(viewscripts.Length, false);

                ViewScriptComparer scriptComparer = new ViewScriptComparer(connection, viewscripts, null, Profile);
                ViewScriptComparisonResult result = scriptComparer.GetCreateResult();

                foreach (ViewScriptInfo info in result.ViewScriptInfos)
                {
                    string parsingError = string.Empty;
                    parsingError += CheckQuery(info, result);
                    parsingError += CheckIndex(connection, info);

                    Viewscript storedscript = Profile.Viewscripts.FirstOrDefault(vs => vs.FileName == info.ViewScript.FileInfo.FullName);
                    storedscript.ParsingError = parsingError;
                    storedscript.ParsingState = string.IsNullOrEmpty(parsingError) ? true : false;
                    storedscript.SaveOrUpdate();

                    progress.StepDone();
                }
            }
        }

        /// <summary>
        /// Checks the physical existance of the tables and fields in the query part of the ViewScript
        /// </summary>
        /// <param name="info">ViewScriptInfo</param>
        /// <param name="result">ViewScriptComparisonResult</param>
        /// <returns>error string</returns>
        private string CheckQuery(ViewScriptInfo info, ViewScriptComparisonResult result)
        {
            string retValue = string.Empty;

            if (info.Errors != null && info.Errors.SyntaxErrors != null && info.Errors.SyntaxErrors.Count > 0)
            {
                retValue = info.Errors.SyntaxErrors.Aggregate((current, next) => current + "   " + next);
            }

            List<string> missingTables = result.GetMissingTables(info);
            if (missingTables.Count > 0)
            {
                retValue += "  Missing tables in query: " + missingTables.Aggregate((current, next) => current + "," + next);
            }
            else
            {
                Dictionary<List<string>, List<string>> missingFields = result.GetMissingColumns(info);
                if (missingFields.Count > 0)
                {
                    string fields = string.Empty;
                    foreach (KeyValuePair<List<string>, List<string>> item in missingFields)
                    {
                        List<string> tables = item.Key;
                        string table = string.Empty;
                        if (tables.Count > 0)
                            table = tables[0];

                        item.Value.ForEach(f => fields += "[" + table + "].[" + f + "], ");
                    }

                    retValue += "  Missing fields in query: " + fields;
                }
            }

            return retValue;
        }

        /// <summary>
        /// Checks the physical existance of the tables and fields occuring in the property indizies of the ViewScripts
        /// </summary>
        /// <param name="connection">database connection</param>
        /// <param name="info">ViewScriptInfo</param>
        /// <returns>error string</returns>
        private string CheckIndex(IDatabase connection, ViewScriptInfo info)
        {
            string retValue = string.Empty;

            Dictionary<string, Dictionary<string, bool>> tablesInIndex = ParseIndexXml(info);
            List<string> missingTablesInIndex = CheckTableExistance((DatabaseBase)connection, tablesInIndex);
            if (missingTablesInIndex.Count > 0)
            {
                retValue += "  Missing tables in index: " + missingTablesInIndex.Aggregate((current, next) => current + "," + next);
            }
            else
            {
                List<string> missingFieldsInIndex = CheckFieldExistance((DatabaseBase)connection, tablesInIndex);
                if (missingFieldsInIndex.Count > 0)
                {
                    retValue += "  Missing fields in index: " + missingFieldsInIndex.Aggregate((current, next) => current + "," + next);
                }
            }

            return retValue;
        }

        /// <summary>
        /// Checks the existance of the tables of the input datastructure
        /// </summary>
        /// <param name="database">database</param>
        /// <param name="tables">input datastructure</param>
        /// <returns>a list of strings containing the tables of the input datastructure that don't exist in the database</returns>
        private List<string> CheckTableExistance(IDatabase database, Dictionary<string, Dictionary<string, bool>> tables)
        {
            string getTables = string.Format(
                @"SELECT
                TABLE_NAME
                FROM 
                information_schema.tables
                WHERE
                TABLE_SCHEMA = '{0}' 
                AND 
                TABLE_TYPE = 'BASE TABLE'",
                            database.DbConfig.DbName);

            List<string> tablesInDb = new List<string>();
            using (IDataReader reader = database.ExecuteReader(getTables))
            {
                while (reader.Read())
                    tablesInDb.Add(((string)reader[0]).ToLower());
            }

            List<string> missingtables = tables.Select(item => item.Key).Except(tablesInDb).ToList();

            return missingtables;
        }

        /// <summary>
        /// Checks the existance of the fields of the input datastructure
        /// </summary>
        /// <param name="database">database</param>
        /// <param name="tables">input datastructure</param>
        /// <returns>a list of strings containing the fields of the input datastructure that don't exist in the database</returns>
        private List<string> CheckFieldExistance(IDatabase database, Dictionary<string, Dictionary<string, bool>> tables)
        {
            string getColumns = string.Format(
                @"SELECT
                TABLE_NAME,
                COLUMN_NAME
                FROM 
                information_schema.COLUMNS
                WHERE
                TABLE_SCHEMA = '{0}'",
                            database.DbConfig.DbName);

            List<string> fieldsInDb = new List<string>();
            using (IDataReader reader = database.ExecuteReader(getColumns))
            {
                while (reader.Read())
                    fieldsInDb.Add("[" + ((string)reader[0]).ToLower() + "].[" + ((string)reader[1]).ToLower() + "]");
            }

            List<string> fieldsInIndex = new List<string>();
            foreach (KeyValuePair<string, Dictionary<string, bool>> table in tables)
                foreach (KeyValuePair<string, bool> field in table.Value)
                    fieldsInIndex.Add("[" + table.Key + "].[" + field.Key + "]");

            List<string> missingFileds = fieldsInIndex.Except(fieldsInDb).ToList();

            return missingFileds;
        }

        /// <summary>
        /// Parsing index xml and generating datastructure
        /// </summary>
        /// <param name="info">ViewScriptInfo</param>
        /// <returns>Return the hierarchical datastructure of the property Indizies of the ViewScriptInfo</returns>
        private Dictionary<string, Dictionary<string, bool>> ParseIndexXml(ViewScriptInfo info)
        {
            Dictionary<string, Dictionary<string, bool>> retValue = new Dictionary<string, Dictionary<string, bool>>();

            XElement indizies = XElement.Parse(info.ViewScript.Indizes);
            IEnumerable<XElement> tables = indizies.Elements();
            foreach (var table in tables)
            {
                string tableName = ((string)table.Attribute("Name")).ToLower();

                if (!retValue.ContainsKey(tableName))
                    retValue.Add(tableName, new Dictionary<string, bool>());
                Dictionary<string, bool> innerDict = retValue[tableName];

                foreach (var idx in table.Elements())
                {
                    string fields = (string)idx.Attribute("Fields");
                    string[] fieldArray = fields.Split(';');
                    foreach (string fieldName in fieldArray)
                        if (!innerDict.ContainsKey(fieldName.ToLower()))
                            innerDict.Add(fieldName.ToLower(), true);
                }
            }

            return retValue;
        }

        #endregion


        public void GetExtendedColumnInformations()
        {
            ExtendedColumnInformationModel model = new ExtendedColumnInformationModel();
            DlgExtendedColumnInformation dlg = new DlgExtendedColumnInformation() { Owner = _mainWindow, DataContext = model };
            model.Owner = dlg;
            dlg.ShowDialog();
        }

    }

}
