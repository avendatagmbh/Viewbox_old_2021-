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
using System.Windows.Navigation;
using System.Windows.Shapes;
using DbComparisonV2.Models;
using System.ComponentModel;
using DbAccess;
using DBComparisonBusiness.Business;
using DbComparisonV2.DocumentGenerator;
using DbComparisonV2.UIHelper;

namespace DbComparisonV2
{
    public enum CompareMode 
    { 
        Database, ViewScript
    }
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        BackgroundWorker _bgWorker;
        CompareMode _compareMode = CompareMode.Database;

        #region Constructors
        public MainWindow()
        {
            InitializeComponent();

#if DEBUG
            var compareConfig = new DatabaseCompareConfig()
            {
                Object1 = new DatabaseConfig("MySQL"),
                Object2 = new DatabaseConfig("MySQL")
            };
            compareConfig.Object1.Hostname = "berd-10013";
            compareConfig.Object1.Password = "avendata";
            compareConfig.Object1.Username = "root";
            compareConfig.Object1.Port = 3306;
            compareConfig.Object1.DbName = "viewbox";

            compareConfig.Object2.Hostname = "berd-10013";
            compareConfig.Object2.Password = "avendata";
            compareConfig.Object2.Username = "root";
            compareConfig.Object2.Port = 3306;
            compareConfig.Object2.DbName = "viewbox_wls";
            DataContext = compareConfig;
#endif
        }
        public MainWindow(DatabaseCompareConfig comparerConfig) :this()
        {
            this.DataContext = comparerConfig;
        }
        #endregion

        #region Properties

        public ICompareConfigBase<ICompareConfig,ICompareConfig> Config
        {
            get
            {
                return (ICompareConfigBase<ICompareConfig, ICompareConfig>)DataContext;
            }
            set
            {
                DataContext = value;
            }
        }
        #endregion

        #region controls event handlers
        private void btnBrowseDirectory_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (txtOutputDir.Text.Length > 0)
            {
                dialog.SelectedPath = txtOutputDir.Text;
            }

            dialog.Description = "Csv Ausgabe Verzeichnis auswählen...";
            dialog.ShowNewFolderButton = true;

            if (dialog.ShowDialog()== System.Windows.Forms.DialogResult.OK)
            {
                txtOutputDir.Text = dialog.SelectedPath;
                txtOutputDir.SelectionStart = txtOutputDir.Text.Length;
            }
        }

        private void btnStartComparison_Click(object sender, RoutedEventArgs e)
        {
            if (Config == null) throw new InvalidOperationException("DataContext contains no DatabaseComparerConfig instance.");

            try
            {
                UpdateChildrenBindings();
                DoComparison(Config);

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Fehler beim Herstellen der Datenbankverbindung:" + Environment.NewLine + ex.Message);
            }
        }
        #endregion

        #region private members
        private ICompareConfigBase<ICompareConfig, ICompareConfig> GetCompareConfig(CompareMode compareMode) 
        { 
            _compareMode = compareMode;
            switch (compareMode)
            { 
                case CompareMode.Database:
                    return new DatabaseCompareConfig() { Object1 = new DatabaseConfig("MySQL"), Object2 = new DatabaseConfig("MySQL") };
                case CompareMode.ViewScript:
                    return new ViewScriptComparerConfig() { Object1 = new DatabaseConfig("MySQL"), Object2 = new ViewScriptConfig() };
            }
            return null;
        }
        private void UpdateCompareMode(CompareMode compareMode)
        {
            Config = GetCompareConfig(compareMode);
        }

        bool SaveConfiguration()
        {
            if (Config == null) return false;

            return DbConfigurationCache.SaveOrCreateConfiguration(Config as DatabaseCompareConfig);
        }
        void RefreshDbConfigurationList(DatabaseCompareConfig config)
        {
            ((ObjectDataProvider)FindResource("dbConfigChoiceProvider")).Refresh();
            cboConfigChoice.SelectedIndex = -1;
            cboConfigChoice.SelectedItem = config;
            cboConfigChoice.SelectedValue = config.ConfigName;
        }
        /// <summary>
        /// solves some issue with two way binding on combobox in CtrlDbConfig (any better?)
        /// </summary>
        void UpdateChildrenBindings()
        {
            switch (_compareMode)
            {
                case CompareMode.Database:
                    ctlDbConfig1.UpdateBindings();
                    ctlDbConfig2.UpdateBindings();
                    break;
                case CompareMode.ViewScript:
#if DEBUG
                    (Config.Object1 as DatabaseConfig).Hostname = "localhost";
                    (Config.Object1 as DatabaseConfig).Username = "avandata";
                    (Config.Object1 as DatabaseConfig).Password = "avandata";
                    ctlDbConfig3.cboDbName.SelectedValue = "sap_wesslingen_020";
#endif
                    ctlDbConfig3.UpdateBindings();
                    break;
            }
        }
        /// <summary>
        /// process the comparison and create a cache of the config at completion
        /// </summary>
        /// <param name="config"></param>
        private void DoComparison(ICompareConfigBase<ICompareConfig, ICompareConfig> config)
        {
            ICompareConfig dbConfig1 = config.Object1;
            ICompareConfig dbConfig2 = config.Object2;

            _bgWorker = new BackgroundWorker();
            DoWorkEventHandler work = null;
            RunWorkerCompletedEventHandler workCompleted = null;
            if (config is DatabaseCompareConfig)
            {
                work = new DoWorkEventHandler(bgWorker_DoWork);
                workCompleted = new RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
            }
            else if(config is ViewScriptComparerConfig)
            {
                work = new DoWorkEventHandler(ViewScriptWorker_DoWork);
                workCompleted = new RunWorkerCompletedEventHandler(ViewScriptWorker_RunWorkerCompleted);
                dbConfig2 = (config as ViewScriptComparerConfig).Object2; // using ICompareConfigBase for Object2 returns null
            }
            _bgWorker.DoWork += work;
            _bgWorker.RunWorkerCompleted += workCompleted;
            _bgWorker.ProgressChanged += (s, e) => { progressBar.Value = e.ProgressPercentage; };

            this.IsEnabled = false;
            progressBar.Minimum = 1;
            progressBar.Maximum = 100;
            progressBar.Value = 1;
            progressBar.Visibility = System.Windows.Visibility.Visible;
            _bgWorker.WorkerReportsProgress = true;
            _bgWorker.RunWorkerAsync(new[] { dbConfig1, dbConfig2 });

        }
        void ViewScriptWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) { 
            this.IsEnabled = true;
            if (!e.Cancelled && e.Error == null)
            {
                MessageBox.Show(this, "Comparison document generated.", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (e.Error != null)
                MessageBox.Show(this, "Es ist ein Fehler aufgetreten:" + Environment.NewLine + e.Error.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            progressBar.Visibility = System.Windows.Visibility.Hidden;
        }
        void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IsEnabled = true;
            if (!e.Cancelled && e.Error == null)
            {
                if (SaveConfiguration())
                    RefreshDbConfigurationList(Config as DatabaseCompareConfig);

                MessageBox.Show(this, "Comparison document generated.", "", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            if (e.Error != null)
                MessageBox.Show(this, "Es ist ein Fehler aufgetreten:" + Environment.NewLine + e.Error.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            progressBar.Visibility = System.Windows.Visibility.Hidden;
        }
        string GetTextBoxValue(TextBox txt)
        {
            if (txt.Dispatcher.CheckAccess())
                return txt.Text;
            else
                return txt.Dispatcher.Invoke((Func<string>)(() => { return GetTextBoxValue(txt); })).ToString();
        }
        void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (GetTextBoxValue(txtOutputDir).Length == 0)
                throw new Exception("Kein Ausgabe Verzeichnis angegeben");

            DatabaseConfig dbConfig1 = ((ICompareConfig[])e.Argument)[0] as DatabaseConfig;
            DatabaseConfig dbConfig2 = ((ICompareConfig[])e.Argument)[1] as DatabaseConfig;

            using (IDatabase conn1 = ConnectionManager.CreateDbFromConfig(dbConfig1))
            {
                conn1.Open();
                using (IDatabase conn2 = ConnectionManager.CreateDbFromConfig(dbConfig2))
                {
                    conn2.Open();
                    DatabaseComparer dbComparer = new DatabaseComparer(conn1, conn2, _bgWorker);
                    string dir = GetTextBoxValue(txtOutputDir); if (!dir.EndsWith("\\")) dir += "\\";
                    _bgWorker.ReportProgress(50);

                    using (var docGen = DocumentFactory.GetFrom<ExcelDBCompareResultGenerator, IComparisonResult>(dbComparer.GetCreateResult()))
                    {
                        _bgWorker.ReportProgress(75);

                        docGen.WriteToFile(System.IO.Path.Combine(dir , string.Format("db_comparison_{0}_{1}", conn1.DbConfig.DbName , conn2.DbConfig.DbName)));
                    }
                    _bgWorker.ReportProgress(100);
                }
            }
        }
        void ViewScriptWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (GetTextBoxValue(txtOutputDir).Length == 0)
                throw new Exception("Kein Ausgabe Verzeichnis angegeben");

            DatabaseConfig dbConfig1 = ((ICompareConfig[])e.Argument)[0] as DatabaseConfig;
            ViewScriptConfig viewsConfig = ((ICompareConfig[])e.Argument)[1] as ViewScriptConfig;

            using (IDatabase conn1 = ConnectionManager.CreateDbFromConfig(dbConfig1))
            {
                conn1.Open();
                ViewScriptComparer dbComparer = new ViewScriptComparer(conn1, viewsConfig.ScriptFiles.ToArray(), _bgWorker);

                List<string> syntaxError = dbComparer.GetScriptsVithoutObjectTree();
                if (syntaxError.Count > 0) {
                    MessageBox.Show("You may have error in the follwing scripts: " + Environment.NewLine + syntaxError.Aggregate((current, next) => current + Environment.NewLine + next),
                                    "Syntax error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string dir = GetTextBoxValue(txtOutputDir); if (!dir.EndsWith("\\")) dir += "\\";
                _bgWorker.ReportProgress(50);

                using (var docGen = DocumentFactory.GetFrom<ExcelViewCompareResultGenerator, IViewComparisonResult>(dbComparer.GetCreateResult()))
                {
                    _bgWorker.ReportProgress(75);

                    docGen.WriteToFile(System.IO.Path.Combine(dir, string.Format("db_comparison_{0}_views", conn1.DbConfig.DbName)));
                }
                _bgWorker.ReportProgress(100);
            }
        }
        private void cboConfigChoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboConfigChoice.SelectedIndex < 1)
            {
                DataContext = new DatabaseCompareConfig() { Object1 = new DatabaseConfig("MySQL"), Object2 = new DatabaseConfig("MySQL") };
                ctlDbConfig1.ClearDatabaseList();
                ctlDbConfig2.ClearDatabaseList();
            }
            else
            {
                //this.InvalidateVisual();
                var newContext = cboConfigChoice.SelectedItem;

                DataContext = newContext;
                ctlDbConfig1.RefreshDatabaseList();
                ctlDbConfig2.RefreshDatabaseList();
            }
        
        }
        private void compareModeTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabCtrl = e.Source as TabControl;

            // change Compare mode if tab change 
            if(tabCtrl != null)
                switch ((tabCtrl.SelectedValue as TabItem).Name)
                {
                    case "tabDatabaseCompare":
                        UpdateCompareMode(CompareMode.Database);
                        break;
                    case "tabViewScriptCompare":
                        UpdateCompareMode(CompareMode.ViewScript);
                        break;
               }
        }
        #endregion




    }
}
