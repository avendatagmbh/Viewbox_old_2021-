using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ViewBuilderBusiness.Structures.Config;
using Utils;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.Specialized;
using ViewBuilderBusiness.Persist;
using ViewBuilderCommon;
using System.Threading;
using SystemDb;
using ViewboxMassTableArchivingTool;
using System.Collections.ObjectModel;
using SqlParser;
using DbAccess;
using System.Data;
using System.IO;


namespace ViewBuilder.Windows
{
    /// <summary>
    /// Interaction logic for DlgArchivingTableSelector.xaml
    /// </summary>
    public partial class DlgArchivingTableSelector : Window, INotifyPropertyChanged
    {
        public delegate void FunctionDelegate(List<string> param);

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        #region [ ListBoxPropertyContainer ]
        /// <summary>
        /// Datasource for the listbox.
        /// </summary>
        public class ListBoxPropertyContainer : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            //One table from profileconfig's viewbox's tables.
            public ITableObject Table { get; set; }

            private bool m_IsChecked;
            /// <summary>
            /// Indicates whether the listbox item is checked or not.
            /// </summary>
            public bool IsChecked
            {
                get
                {
                    return m_IsChecked;
                }
                set
                {
                    m_IsChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }

            private bool m_IsArchived;
            /// <summary>
            /// Indicates whether the table in the listbox item is archived or not.
            /// </summary>
            public bool IsArchived
            {
                get
                {
                    return m_IsArchived;
                }
                set
                {
                    m_IsArchived = value;
                    OnPropertyChanged("IsArchived");
                }
            }

            /// <summary>
            /// Creates a new container.
            /// </summary>
            /// <param name="p_Table">The table from viewboxdb to store.</param>
            /// <param name="p_IsChecked">The listbox item's default checked property (usually false).</param>
            public ListBoxPropertyContainer(ITableObject p_Table, bool p_IsChecked)
            {
                Table = p_Table;
                m_IsChecked = p_IsChecked;
                m_IsArchived = p_Table.IsArchived;
            }

            protected void OnPropertyChanged(string name)
            {
                PropertyChangedEventHandler handler = PropertyChanged;

                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }
        } 
        #endregion

        #region [ WorkerArguments ]
        public class WorkerArgumentBase
        {
            /// <summary>
            /// Exception list (set it to null, when there are no checked item in the listbox).
            /// </summary>
            public List<Exception> ExceptionList;

            /// <summary>
            /// Sets default values of the properties in the class.
            /// </summary>
            public WorkerArgumentBase()
            {
                ExceptionList = new List<Exception>();
            }
        }

        /// <summary>
        /// This class hold information between the workerprocess' calls.
        /// </summary>
        public class WorkerArgumentArchiving : WorkerArgumentBase
        {
            /// <summary>
            /// The archiving type (Archive or Restore).
            /// </summary>
            public ViewboxDb.ArchiveType ArchiveType;

            /// <summary>
            /// Sets default values of the properties in the class.
            /// </summary>
            public WorkerArgumentArchiving() : base()
            {
                ArchiveType = ViewboxDb.ArchiveType.Archive;
            }
        } 
        #endregion

        #region [ Properties ]
        /// <summary>
        /// Contains the tables from the datacontext and some additional property for listbox operation.
        /// </summary>
        public ObservableCollection<ListBoxPropertyContainer> PropertyContainer { get; set; }

        /// <summary>
        /// Contains the filtered tables.
        /// </summary>
        public ObservableCollection<ListBoxPropertyContainer> FilteredPropertyContainer { get; set; }

        /// <summary>
        /// Inficates whether the systemdb tables are loaded or not.
        /// </summary>
        private bool TablesLoaded { get; set; }

        //The current profileconfig.
        private ProfileConfig CurrentProfileConfig { get; set; }

        /// <summary>
        /// Contains the filter text
        /// </summary>
        private string _filter;
        public string Filter
        {
            get
            {
                return _filter;
            }

            set
            {
                _filter = value;
                FilterTables();
                OnPropertyChanged("Filter");
            }
        }

        private PopupProgressBar PopupProgress;

        #endregion

        #region [ Delegates, events and event args ]
        public delegate void TableCollectingHandler(object sender, TableCollectingEventArgs e, object progressCalculator);
        public static event TableCollectingHandler TableCollectingStepCompleted;

        /// <summary>
        /// Custom event args, containing archived table's name.
        /// </summary>
        public class TableCollectingEventArgs : EventArgs
        {
            private string m_TableName;
            /// <summary>
            /// The name of the loaded table.
            /// </summary>
            public string TableName { get { return m_TableName; } }            

            /// <summary>
            /// Contructor...
            /// </summary>
            /// <param name="p_TableCount">The name of the loaded table.</param>
            public TableCollectingEventArgs(string p_TableName)
            {
                this.m_TableName = p_TableName;                
            }
        } 
        #endregion

        #region [ Constructor ]
        public DlgArchivingTableSelector(ProfileConfig p_ProfileConfig)
        {
            InitializeComponent();

            CurrentProfileConfig = p_ProfileConfig;
            
            SetContainer();
        }
        #endregion

        #region [ SetContainer ]
        /// <summary>
        /// Sets the PropertyContainer's data.
        /// </summary>
        private void SetContainer()
        {
            PropertyContainer = new ObservableCollection<ListBoxPropertyContainer>();
            FilteredPropertyContainer = new ObservableCollection<ListBoxPropertyContainer>();

            //foreach (ITableObject table in CurrentProfileConfig.ViewboxDb.Tables.Take(1000))
            foreach (ITableObject table in CurrentProfileConfig.ViewboxDb.Tables.OrderBy(element => element.TableName))
            {
                //Adds the table with a deafulat 'not checked' value.
                ListBoxPropertyContainer container = new ListBoxPropertyContainer(table, false);
                PropertyContainer.Add(container);
                FilteredPropertyContainer.Add(container);
            }

            this.DataContext = this;
        } 
        #endregion

        #region [ Click events ]
        /// <summary>
        /// Retrieves the used table list and checks them in the listbox if finds them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnDetect_Click(object sender, RoutedEventArgs e)
        {
            if (lbxTableList != null && lbxTableList.Items != null &&
                lbxTableList.Items.Count > 0)
            {
                WorkerArgumentBase m_Arg = new WorkerArgumentBase();

                ProgressCalculator progress = new ProgressCalculator();
                
                progress.Title = Properties.Resources.ArchivingProgressDialogTitleDetectionStarted;
                progress.SetWorkSteps(CurrentProfileConfig.Viewscripts.Count, false);

                progress.DoWork += (send, ev) => DoFunction(send, ev, CheckTables);
                progress.WorkerSupportsCancellation = true;
                progress.RunWorkerCompleted += progress_DetectingUsedTables_RunWorkerCompleted;
                progress.RunWorkerAsync(m_Arg);
                PopupProgress = new PopupProgressBar() { Owner = this, DataContext = progress };                
                PopupProgress.ShowDialog();
            }
            else
                MessageBox.Show(this,
                                Properties.Resources.ArchivingTableListEmpty,
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
        }

        /// <summary>
        /// Exports all used tables
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnExportUsedTables_Click(object sender, RoutedEventArgs e)
        {
            WorkerArgumentBase m_Arg = new WorkerArgumentBase();

            ProgressCalculator progress = new ProgressCalculator();

            progress.Title = Properties.Resources.ArchivingProgressDialogTitleDetectionStarted;
            progress.SetWorkSteps(CurrentProfileConfig.Viewscripts.Count, false);

            progress.DoWork += (send, ev) => DoFunction(send, ev, Export);
            progress.WorkerSupportsCancellation = true;
            progress.RunWorkerCompleted += progress_DetectingUsedTables_RunWorkerCompleted;
            progress.RunWorkerAsync(m_Arg);
            PopupProgress = new PopupProgressBar() { Owner = this, DataContext = progress };
            PopupProgress.ShowDialog();
        }

        /// <summary>
        /// Calls archiving with 'Archiving' parameter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnArchive_Click(object sender, RoutedEventArgs e)
        {
            OpenTableArchivingSetupDialog(ViewboxDb.ArchiveType.Archive);
        }

        /// <summary>
        /// Calls archiving with 'Restoring' parameter.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            OpenTableArchivingSetupDialog(ViewboxDb.ArchiveType.Restore);
        }

        /// <summary>
        /// Selects all items in the listbox if there is at least one not-checked among them.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAll_Click(object sender, RoutedEventArgs e)
        {
            bool m_CheckedState = PropertyContainer.All(element => element.IsChecked);

            foreach (ListBoxPropertyContainer prop in PropertyContainer)
                prop.IsChecked = !m_CheckedState;
        } 
        
        /// <summary>
        /// Selects all archived items in the listbox and deselects the others.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonArchived_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListBoxPropertyContainer prop in PropertyContainer)
                prop.IsChecked = prop.IsArchived;
        }

        /// <summary>
        /// Selects all non-archived items in the listbox and deselects the others.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonRestored_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListBoxPropertyContainer prop in PropertyContainer)
                prop.IsChecked = !prop.IsArchived;
        }
        #endregion


        #region [ Detect unused tables methods ]

        private void DoFunction(object sender, DoWorkEventArgs e, FunctionDelegate function)
        {
            WorkerArgumentBase m_Arg = (WorkerArgumentBase)e.Argument;

            List<string> m_UsedTablesList = FillUsedTableList(sender, m_Arg);
            function(m_UsedTablesList);

            e.Result = m_Arg;
        }

        /// <summary>
        /// Fills m_UsedTablesList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="m_Arg"></param>
        /// <param name="m_UsedTablesList"></param>
        private List<string> FillUsedTableList(object sender, WorkerArgumentBase m_Arg)
        {
            List<string> retValue = new List<string>();

            SqlParser.UsedTablesCollector m_Collector = new SqlParser.UsedTablesCollector();

            foreach (ProjectDb.Tables.Viewscript script in CurrentProfileConfig.Viewscripts)
            {
                try
                {
                    retValue.AddRange(m_Collector.GetUsedTablesListFromScript(script.Script));

                    (sender as ProgressCalculator).StepDone();
                }
                catch (Exception ex)
                {
                    m_Arg.ExceptionList.Add(ex);
                }
            }

            retValue = retValue.Distinct().ToList();

            return retValue;
        }

        private void progress_DetectingUsedTables_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerArgumentBase m_Arg = (WorkerArgumentBase)e.Result;

            if (m_Arg.ExceptionList == null)
            {
                MessageBox.Show(this,
                                Properties.Resources.ArchivingNoScriptFileForDetection,
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
            else if (m_Arg.ExceptionList.Count > 0)
            {
                MessageBox.Show(this,
                                Properties.Resources.ArchivingUsedTableDetectionUnexpectedError,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else if (e.Error != null)
            {
                MessageBox.Show(this,
                                Properties.Resources.ArchivingUsedTableDetectionError + e.Error.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                MessageBox.Show(this,
                                Properties.Resources.ArchivingUsedTableCollectingCancelled,
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            else
                MessageBox.Show(this,
                                Properties.Resources.ArchivingUsedTableCollectingSuccessful,
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

            PopupProgress.Close();
        }  
        #endregion

        #region [ CheckTables ]
        /// <summary>
        /// Sets IsChecked property in the PropertyContainer's elements when the elements' table's tablename can't be
        /// found on the used table list, even with a '_' prefix.
        /// </summary>
        /// <param name="p_UsedTables"></param>
        private void CheckTables(List<string> p_UsedTables)
        {
            foreach (ListBoxPropertyContainer prop in PropertyContainer)
            {
                if (!p_UsedTables.Any(
                    element => string.Compare(prop.Table.TableName, element, true) == 0 ||
                               string.Compare(prop.Table.TableName, "_" + element, true) == 0))
                    prop.IsChecked = true;
                else
                    prop.IsChecked = false;
            }
        } 
        #endregion

        #region [ GetCheckedTableNames ]
        /// <summary>
        /// Retrieves all the table names with checked status.
        /// </summary>
        /// <returns>List of strings containing the checked tables' name.</returns>
        private List<string> GetCheckedTableNames()
        {
            return PropertyContainer.Where(element => element.IsChecked).Select(
                element => element.Table.TableName).ToList();
        }

        #endregion

        #region [ GetCheckedTableIds ]
        /// <summary>
        /// Retrieves all the table names with checked status.
        /// </summary>
        /// <returns>List of strings containing the checked tables' name.</returns>
        private List<int> GetCheckedTableIds()
        {
            return PropertyContainer.Where(element => element.IsChecked).Select(
                element => element.Table.Id).ToList();
        }

        #endregion

        #region [ Retrieving table list ]
       
        private void progress_TableRetrieving(object sender, DoWorkEventArgs e)
        {
            //this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            //{
            //    TableCollectingStepCompleted += new TableCollectingHandler(DlgArchivingTableSelector_TableCollectingStepCompleted);

            //    IOrderedEnumerable<SystemDb.ITable> m_Tables = CurrentProfileConfig.ViewboxDb.Tables.OrderBy(element => element.TableName);

            //    (sender as ProgressCalculator).SetWorkSteps(m_Tables.Count(), false);

            //    Model.TableList.Clear();

            //    foreach (SystemDb.ITable table in m_Tables)
            //    {
            //        //lbxTableList.Items.Add(new KeyValuePair<string, bool>(table.TableName, table.IsArchived));
            //        Model.TableList.Add(table);

            //        if (TableCollectingStepCompleted != null)
            //            TableCollectingStepCompleted(null, new TableCollectingEventArgs(table.TableName), sender);

            //        (sender as ProgressCalculator).StepDone();
            //    }                
            //}
            //));
        }

        void DlgArchivingTableSelector_TableCollectingStepCompleted(object sender, TableCollectingEventArgs e, object progressCalculator)
        {
            if (progressCalculator != null)
            {
                (progressCalculator as ProgressCalculator).Description = e.TableName + Properties.Resources.ArchivingTableLoaded;
                (progressCalculator as ProgressCalculator).StepDone();
            }
        }
              
        #endregion

        #region [ Table archiving methods ]
        internal void OpenTableArchivingSetupDialog(ViewboxDb.ArchiveType p_ArchiveType)
        {
            try
            {
                //Sets the argument's archiving type property here. Exceptionlist will be used later.
                WorkerArgumentArchiving m_Args = new WorkerArgumentArchiving();
                m_Args.ArchiveType = p_ArchiveType;

                ProgressCalculator progress = new ProgressCalculator();
                progress.Title = Properties.Resources.ArchivingProgressDialogTitleStarted;

                progress.DoWork += progress_TableArchivingSetup;
                progress.WorkerSupportsCancellation = true;
                progress.RunWorkerCompleted += progress_TableArchivingSetup_RunWorkerCompleted;
                progress.RunWorkerAsync(m_Args);
                PopupProgress = new PopupProgressBar() { Owner = this, DataContext = progress };
                PopupProgress.ShowDialog();

            }
            catch (Exception ex)
            {
                MessageBox.Show(this,
                    ((p_ArchiveType == ViewboxDb.ArchiveType.Archive) ?
                        Properties.Resources.ArchivingProgressErrorArchive :
                        Properties.Resources.ArchivingProgressErrorRestore) + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void progress_TableArchivingSetup(object sender, DoWorkEventArgs e)
        {
            //Get the argument.
            WorkerArgumentArchiving m_Args = (WorkerArgumentArchiving)e.Argument;

            //Get the archiving type from the argument.
            ViewboxDb.ArchiveType m_ArchiveType = m_Args.ArchiveType;            

            this.Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
            {
                ProgressCalculator progress = sender as ProgressCalculator;

                //Get the checked tables' name.
                List<int> m_CheckedIds = GetCheckedTableIds();

                if (m_CheckedIds.Count > 0)
                {
                    ArchivingStartedEventArgs m_StartedEventArg = new ArchivingStartedEventArgs(m_CheckedIds.Count);
                    ArchiveRestoreLogic_ArchivingStarted(this, m_StartedEventArg, progress);

                    using (ViewboxDb.ViewboxDb db = new ViewboxDb.ViewboxDb(
                    CurrentProfileConfig.ViewboxDbName + "_temp", CurrentProfileConfig.ViewboxDbName + "_index",
                    CurrentProfileConfig.ViewboxDb))
                    {
                        string m_ConnectionString =
                            "server=" + CurrentProfileConfig.DbConfig.Hostname + ";User Id=" +
                            CurrentProfileConfig.DbConfig.Username + ";password=" +
                            CurrentProfileConfig.DbConfig.Password +
                            ";database=" + CurrentProfileConfig.ViewboxDbName +
                            ";Connect Timeout=1000;Default Command Timeout=0;Allow Zero Datetime=True";

                        db.Connect("MySQL", m_ConnectionString);

                        int m_TableIndex = 0;

                        TableArchivingEventArgs m_Arg;

                        //We need a temporary list of selected tables as for an unknown reason,
                        //when we call ArchiveTable, the profile's table list yields no result...
                        //So, i copied the selected tables into a temp list...
                        List<ITableObject> m_TempList = new List<ITableObject>();
                        while (m_TableIndex < m_CheckedIds.Count)
                        {
                            ITableObject m_TableObject = (CurrentProfileConfig.ViewboxDb.Tables.FirstOrDefault(element =>
                                element.Id == m_CheckedIds[m_TableIndex]).Clone() as ITableObject);

                            m_TempList.Add(m_TableObject);

                            m_TableIndex++;
                        }

                        m_TableIndex = 0;

                        TablesLoaded = false;

                        db.SystemDb.LoadingFinished += new Action(SystemDb_LoadingFinished);

                        while (m_TableIndex < m_CheckedIds.Count)
                        {
                            //Here, get the table by its name from the temp list instead of the profile's table list.
                            ITableObject m_TableObject = m_TempList.FirstOrDefault(element =>
                                element.Id == m_CheckedIds[m_TableIndex]);

                            if (m_TableObject != null)
                            {
                                m_Arg = new TableArchivingEventArgs(m_TableObject.TableName);

                                ArchiveRestoreLogic_TableArchivingStarted(this, m_Arg, progress, m_ArchiveType);

                                try
                                {
                                    //Archive or restore a selected table.
                                    db.ArchiveTable(m_TableObject, m_ArchiveType);

                                    //UpdateTableObjectArchived fails until the table data is not populated in the systemdb.
                                    //An event will set this variable to 'true' when the tables are loaded.
                                    while (!TablesLoaded)
                                    {
                                        Thread.Sleep(100);
                                    }
                                    
                                    //Save the table's information.
                                    db.SystemDb.UpdateTableObjectArchived(m_TableObject, (m_ArchiveType == ViewboxDb.ArchiveType.Archive));

                                    //Get the table's property from the datacontext.
                                    ListBoxPropertyContainer m_Container = PropertyContainer.FirstOrDefault(element => element.Table.Id == m_TableObject.Id);

                                    //Update the IsArchived property of the table.
                                    if (m_Container != null)
                                        m_Container.IsArchived = m_TableObject.IsArchived;
                                }
                                catch (Exception ex)
                                {
                                    //If anything has gone wrong, save the exception into the argument's ExceptionList.
                                    //It will be used later in the progress completed event.
                                    m_Args.ExceptionList.Add(ex);

                                    m_Arg.Success = false;
                                }

                                ArchiveRestoreLogic_TableArchivingFinished(this, m_Arg, progress, m_ArchiveType);
                            }

                            m_TableIndex++;
                        }

                        ArchiveRestoreLogic_ArchivingFinished(this, null, progress);
                    }
                }
                else
                    //Null will indicate that there were no selection in the list.
                    m_Args.ExceptionList = null;
            }
            ));
            
            //Save the argument into the result. That will be used in the completed event.
            e.Result = m_Args;
        }

        /// <summary>
        /// When the tables are loaded in the systemdb, we can set our variable to 'true'.
        /// </summary>
        void SystemDb_LoadingFinished()
        {
            TablesLoaded = true;
        }

        void ArchiveRestoreLogic_TableArchivingFinished(object sender, ViewboxMassTableArchivingTool.TableArchivingEventArgs e,
            object progressCalculator, ViewboxDb.ArchiveType p_ArchiveType)
        {
            if (progressCalculator != null)
            {
                (progressCalculator as ProgressCalculator).Description = e.TableName + " " + ((p_ArchiveType == ViewboxDb.ArchiveType.Archive) ?
                                    Properties.Resources.ArchivingArchiving :
                                    Properties.Resources.ArchivingRestoring) + " " +
                                    ((e.Success) ? Properties.Resources.Finished : Properties.Resources.Failed) + ".";
                (progressCalculator as ProgressCalculator).StepDone();
            }
        }

        void ArchiveRestoreLogic_TableArchivingStarted(object sender, ViewboxMassTableArchivingTool.TableArchivingEventArgs e,
            object progressCalculator, ViewboxDb.ArchiveType p_ArchiveType)
        {
            if (progressCalculator != null)
                (progressCalculator as ProgressCalculator).Description = e.TableName + " " + ((p_ArchiveType == ViewboxDb.ArchiveType.Archive) ?
                                    Properties.Resources.ArchivingArchiving :
                                    Properties.Resources.ArchivingRestoring) + " " +
                                    Properties.Resources.Started + ".";
        }

        void ArchiveRestoreLogic_ArchivingFinished(object sender, EventArgs e, object progressCalculator)
        {
            //if (progressCalculator != null)
            //    (progressCalculator as ProgressCalculator).Title = "as";
        }

        void ArchiveRestoreLogic_ArchivingStarted(object sender, ViewboxMassTableArchivingTool.ArchivingStartedEventArgs e,
            object progressCalculator)
        {
            if (progressCalculator != null)
                (progressCalculator as ProgressCalculator).SetWorkSteps(e.TableCount, false);
        }

        private void progress_TableArchivingSetup_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            WorkerArgumentArchiving m_Args = (WorkerArgumentArchiving)e.Result;

            ViewboxDb.ArchiveType m_ArchiveType = m_Args.ArchiveType;

            if (m_Args.ExceptionList == null)
            {
                MessageBox.Show(this,
                                Properties.Resources.ArchivingTableListEmpty,
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
            else if (m_Args.ExceptionList.Count > 0)
            {
                MessageBox.Show(this,
                                string.Format(Properties.Resources.ArchivingUnexpectedError,
                                ((m_ArchiveType == ViewboxDb.ArchiveType.Archive) ?
                                    Properties.Resources.ArchivingArchiving :
                                    Properties.Resources.ArchivingRestoring)),
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else if (e.Error != null)
            {
                MessageBox.Show(this,
                                ((m_ArchiveType == ViewboxDb.ArchiveType.Archive) ?
                                    Properties.Resources.ArchivingTableArchivingErrorArchive :
                                    Properties.Resources.ArchivingTableArchivingErrorRestore) + e.Error.Message,
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
            else if (e.Cancelled)
            {
                MessageBox.Show(this,
                                ((m_ArchiveType == ViewboxDb.ArchiveType.Archive) ?
                                    Properties.Resources.ArchivingTableArchivingCancelledArchive :
                                    Properties.Resources.ArchivingTableArchivingCancelledRestore),
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            else
                MessageBox.Show(this,
                                ((m_ArchiveType == ViewboxDb.ArchiveType.Archive) ?
                                    Properties.Resources.ArchivingTableArchivingSuccessfulArchive :
                                    Properties.Resources.ArchivingTableArchivingSuccessfulRestore),
                                "",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

            SetContainer();
            PopupProgress.Close();
        } 
        #endregion        

        #region Detecting tables occuring in stored procedures

        /// <summary>
        /// Eventhandler that ticks the checkboxes of tables that are not used in any stored procedures registered in table Tables
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">eventargs</param>
        private void ButtonParseProcedures_Click(object sender, RoutedEventArgs e)
        {
            List<string> missingProcedures;

            CheckTables(SelectTablesInProcedures(out missingProcedures));

            if (missingProcedures.Count > 0)
            {
                string warning = string.Format("The following stored procedures not exist: {0}", missingProcedures.Aggregate((current, next) => current + ", " + next));
                MessageBox.Show(warning, "Warning");
            }
        }

        /// <summary>
        /// Returns the tables that are used in any stored procedures
        /// </summary>
        /// <param name="missingProcedures">List of missing stored procedures, these are ignored by parsing</param>
        /// <returns>List of tables</returns>
        private List<string> SelectTablesInProcedures(out List<string> missingProcedures)
        {
            Dictionary<string, string> existingProcedures = ListStoredProcedures(out missingProcedures);

            SqlParser.UsedTablesCollector tableCollector = new SqlParser.UsedTablesCollector();
            List<string> tablesInProcedures = new List<string>();
            existingProcedures.ToList().ForEach(sp => tablesInProcedures.AddRange(tableCollector.GetUsedTablesListFromScript(sp.Value)));
            tablesInProcedures = tablesInProcedures.Distinct().ToList();

            return tablesInProcedures;
        }

        /// <summary>
        /// Returns the stored procedures
        /// </summary>
        /// <returns>Key: stored procedure name, Value: stored procedure declaration</returns>
        private Dictionary<string, string> ListStoredProcedures(out List<string> missingProcedures)
        {
            Dictionary<string, string> retValue = new Dictionary<string, string>();

            List<string> existingProcedures = ListExistingProcedures();
            List<string> registeredProcedures = ListRegisteredStoredProcedures();

            missingProcedures = registeredProcedures.Except(existingProcedures).ToList();
            registeredProcedures.Intersect(existingProcedures).ToList().ForEach(sp => retValue[sp] = GetStoredProcedureText(sp));

            return retValue;
        }

        /// <summary>
        /// Returns the stored procedures registered in ViewboxDb
        /// </summary>
        /// <returns>The list of stored procedure names</returns>
        private List<string> ListRegisteredStoredProcedures()
        {   
            List<string> retValue = new List<string>();

            using (IDatabase database = CurrentProfileConfig.ViewboxDb.ConnectionManager.GetConnection())
            {
                string listStoredProceduresCommandString =
                    @"select
                     issue_extensions.command
                    from 
                     tables
                    join 
                     issue_extensions on tables.id = issue_extensions.ref_id
                    where 
                     tables.type = 3 and
                     issue_extensions.obj_id = 0";

                using (IDataReader reader = database.ExecuteReader(listStoredProceduresCommandString))
                {
                    while (reader.Read())
                    {
                        retValue.Add((string)reader[0]);
                    }
                }
            }
            
            return retValue;
        }

        /// <summary>
        /// Returns the stored procedures that phisicaly exist in the database
        /// </summary>
        /// <returns>List of stored procedure names</returns>
        private List<string> ListExistingProcedures()
        {
            List<string> retValue = new List<string>();

            using (IDatabase database = CurrentProfileConfig.ConnectionManager.GetConnection())
            {
                string listExistingProcedures = string.Format("show procedure status where Db = '{0}'", database.Connection.Database);

                using (IDataReader reader = database.ExecuteReader(listExistingProcedures))
                {
                    while (reader.Read())
                    {
                        retValue.Add((string)reader[1]);
                    }
                }
            }
            
            return retValue;
        }

        /// <summary>
        /// Returns the stored procedure declaration by the name
        /// </summary>
        /// <param name="spName">Stored procedure name</param>
        /// <returns>declaration of the stored procedure</returns>
        private string GetStoredProcedureText(string spName)
        {
            string retValue = string.Empty;

            using (IDatabase database = CurrentProfileConfig.ConnectionManager.GetConnection())
            {
                string getProcedureCommandString = string.Format("show create procedure `{0}`", spName);

                try
                {
                    using (IDataReader reader = database.ExecuteReader(getProcedureCommandString))
                    {
                        if (reader.Read())
                        {
                            retValue = (string)reader[2];
                        }
                    }
                }
                catch
                {
                    // SP does not exist
                }
            }

            return retValue;
        }

        #endregion

        #region Export to CSV

        /// <summary>
        /// Export button event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            ExportToCSV(GetCheckedTableNames());
        }

        private void Export(List<string> tableNames)
        {
            ExportToCSV(FilterTableList(tableNames));
        }

        /// <summary>
        /// Cuts the '-' prefixes, filters the same strings, does ordering
        /// </summary>
        /// <param name="tableNames">intput list</param>
        /// <returns>output list</returns>
        private List<string> FilterTableList(List<string> tableNames)
        {
            List<string> retValue = new List<string>();

            foreach (string table in tableNames)
            {
                string tmp = table;
                if (table.StartsWith("_"))
                    tmp = table.Substring(1);
                retValue.Add(tmp);
            }

            retValue = retValue.Distinct().ToList();
            retValue.Sort();

            return retValue;
        }

        /// <summary>
        /// Exports the selected table to CSV
        /// </summary>
        private void ExportToCSV(List<string> tableNames)
        {
            string fileContent = string.Empty;
            if (tableNames.Count > 0)
                fileContent = tableNames.Aggregate((current, next) => current + ";\r\n" + next);

            string fileName = GetCSVFileName();
            if (!string.IsNullOrEmpty(fileName))
            {
                try
                {
                    File.WriteAllText(fileName, fileContent);
                }
                catch (IOException ex)
                {
                    MessageBox.Show("Unable to open file");
                }
            }
        }

        /// <summary>
        /// Gets the target file name through save file dialog
        /// </summary>
        /// <returns>full target path</returns>
        private string GetCSVFileName()
        {
            string retValue = string.Empty;

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "tables.csv";
            dlg.DefaultExt = ".csv";
            dlg.Filter = "CSV (Comma delimited) (.csv)|*.csv";

            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                retValue = dlg.FileName;
            }

            return retValue;
        }

        #endregion

        #region Filtering logic

        private void FilterTables()
        {
            FilteredPropertyContainer.Clear();
            PropertyContainer.Where(p => p.Table.TableName.Contains(Filter)).OrderBy(p=>p.Table.Name).ToList().ForEach(t => FilteredPropertyContainer.Add(t));
            OnPropertyChanged("FilteredPropertyContainer");
        }

        #endregion
    }
}