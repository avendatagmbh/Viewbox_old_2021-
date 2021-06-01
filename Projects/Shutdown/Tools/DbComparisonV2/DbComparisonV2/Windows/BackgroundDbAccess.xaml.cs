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
using System.Threading;
using DbAccess.Structures;
using DbAccess;
using System.Windows.Threading;

namespace DbComparisonV2.Windows
{
    /// <summary>
    /// Interaktionslogik für BackgroundDbAccess.xaml
    /// </summary>
    public partial class BackgroundDbAccess : Window
    {
        private Thread _workerThread;

        private string _dbType;
        private string _hostname;
        private string _userName;
        private string _password;
        private DbConfig _config;
        DispatcherTimer _workerThreadTimer;

        IDatabase _database;
        string _schema;

        private List<string> mSchemas;

        public List<string> Schemas
        {
            get { return mSchemas; }
        }
        private List<string> mTables;

        public List<string> Tables
        {
            get { return mTables; }
        }

        public BackgroundDbAccess(string dbType, string host, string user, string pwd)
        {
            InitializeComponent();

            _dbType = dbType;
            _hostname = host;
            _userName = user;
            _password = pwd;
            _config = null;
        }

        public BackgroundDbAccess(DbConfig config)
        {
            InitializeComponent();

            _dbType = config.DbType;
            _hostname = config.Hostname;
            _userName = config.Username;
            _password = config.Password;
            _config = config;
        }

        public void ReadSchemas()
        {
            _workerThread = new Thread(ReadSchemas_Work);
        }

        public void TestConnection()
        {
            _workerThread = new Thread(TestConnection_Work);
        }

        public void ReadTables(string sSchema)
        {
            _schema = sSchema;
            _workerThread = new Thread(ReadTables_Work);
        }
        bool? CurrentDialogResult {
            get {
                if (Dispatcher.CheckAccess())
                    return DialogResult;
                else
                    return (bool?)Dispatcher.Invoke((Func<bool?>)(() => { return DialogResult; }));
            }
            set {
                if (Dispatcher.CheckAccess())
                    DialogResult = value;
                else
                    Dispatcher.Invoke((Action)(() => { DialogResult = value; }));
            }
        }
        private void ReadSchemas_Work()
        {

            try
            {
                if (_config == null)
                {
                    _config = new DbConfig(_dbType)
                    {
                        Username = _userName,
                        Hostname = _hostname,
                        Password = _password
                    };
                }
                _database = ConnectionManager.CreateDbFromConfig(_config);

                UpdateState("Verbinde mit Datenbank...");
                _database.Open();

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                if (CurrentDialogResult == null)
                {
                    ShowError(
                        "Keine Verbindung möglich:" + Environment.NewLine + ex.Message,
                        "Datenbankschemata auslesen");
                }

                return;
            }

            try
            {
                UpdateState("Lese Datebankliste...");
                mSchemas = _database.GetSchemaList();
                UpdateState("Fertig...");


                if (!CurrentDialogResult.HasValue)
                    CurrentDialogResult = true;

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                ShowError(
                    "Fehler beim Lesen der Datenbankschemata:" + Environment.NewLine + ex.Message,
                    "Datenbankschemata auslesen");
            }
            finally
            {
                _database.Close();
            }
        }

        private void TestConnection_Work()
        {
            if (_config == null)
            {
                _database = ConnectionManager.CreateConnection(_dbType, _hostname, _userName, _password);
            }
            else
            {
                _database = ConnectionManager.CreateDbFromConfig(_config);
            }

            try
            {
                UpdateState("Verbinde mit Datenbank...");
                _database.Open();

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                if (!DialogResult.HasValue || !DialogResult.Value)
                {
                    ShowError(
                        "Keine Verbindung möglich:" + Environment.NewLine + ex.Message,
                        "Verbindung testen");
                }

                return;
            }
            finally
            {
                _database.Close();
            }
        }

        private void UpdateState(string state) 
        {
            if (lblStatus.Dispatcher.CheckAccess())
                lblStatus.Content = state;
            else
                lblStatus.Dispatcher.Invoke((Action)delegate() { UpdateState(state); });
        }

        private void ShowError(string msg, string caption)
        {
            if (this.Dispatcher.CheckAccess())
            {
                this.Hide();
                MessageBox.Show(
                    this,
                    msg,
                    caption, MessageBoxButton.OK, MessageBoxImage.Error);

                DialogResult = false;
            }
            else
                this.Dispatcher.Invoke((Action)delegate() { ShowError(msg, caption); });
        }

        private void ReadTables_Work()
        {
            if (_config == null)
            {
                _database = ConnectionManager.CreateConnection(_dbType, _hostname, _userName, _password);
            }
            else
            {
                _database = ConnectionManager.CreateDbFromConfig(_config);
            }

            try
            {
                UpdateState("Verbinde mit Datenbank...");
                _database.Open();

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                ShowError(
                    "Keine Verbindung möglich:" + Environment.NewLine + ex.Message,
                    "Tabellenliste auslesen");

                return;
            }

            try
            {
                UpdateState("Lese Tabellenliste...");
                mTables = _database.GetTableList(_schema);
                UpdateState("Fertig...");

                if (CurrentDialogResult == null)
                    CurrentDialogResult = true;

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                ShowError(
                    "Fehler beim Lesen der Tabellenliste:" + Environment.NewLine + ex.Message,
                    "Tabellenliste auslesen");

            }
            finally
            {
                _database.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            _workerThreadTimer.Stop();
            DialogResult = false;
            _workerThread.Abort();
            _database.CancelCommand();
            this.Close();
        }

        void _workerThreadTimer_Tick(object sender, EventArgs e)
        {
            if (!_workerThread.IsAlive)
            {
                _workerThreadTimer.Stop();
                _workerThread = null;
                // Closes on the UI thread
                Dispatcher.Invoke((Action)(() => { this.Close(); }));
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            _workerThreadTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(20) };
            _workerThreadTimer.Tick += new EventHandler(_workerThreadTimer_Tick);

            _workerThreadTimer.Start();
            _workerThread.Start();
        }
    }
}
