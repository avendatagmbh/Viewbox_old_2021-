using System;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace VPNTool {
    public class VpnTool : INotifyPropertyChanged, IDisposable {
        #region Properties

        #region VpnName
        private string _vpnName;

        public string VpnName {
            get { return _vpnName; }
            set {
                if (_vpnName != value) {
                    _vpnName = value;
                    OnPropertyChanged("VpnName");
                }
            }
        }
        #endregion VpnName

        #region UserName
        private string _userName;

        public string UserName {
            get { return _userName; }
            set {
                if (_userName != value) {
                    _userName = value;
                    OnPropertyChanged("UserName");
                }
            }
        }
        #endregion UserName

        #region Password
        private string _password;

        public string Password {
            get { return _password; }
            set {
                if (_password != value) {
                    _password = value;
                    OnPropertyChanged("Password");
                }
            }
        }
        #endregion Password

        #region IsConnected
        private bool _isConnected;

        public bool IsConnected {
            get { return _isConnected; }
            set {
                if (_isConnected != value) {
                    _isConnected = value;
                    OnPropertyChanged("IsConnected");
                }
            }
        }
        #endregion IsConnected

        #region LastErrorMessage
        private string _lastErrorMessage;

        public string LastErrorMessage {
            get { return _lastErrorMessage; }
            set {
                if (_lastErrorMessage != value) {
                    _lastErrorMessage = value;
                    OnPropertyChanged("LastErrorMessage");
                }
            }
        }
        #endregion LastErrorMessage

        #region BackWorker
        private BackgroundWorker _backWorker;

        public BackgroundWorker BackWorker {
            get { return _backWorker; }
            set {
                if (_backWorker != value) {
                    _backWorker = value;
                    OnPropertyChanged("BackWorker");
                }
            }
        }
        #endregion BackWorker

        #region CheckTime
        private int _checkTime;

        public int CheckTime {
            get { return _checkTime; }
            set {
                if (value < 1)
                    value = 1;

                if (_checkTime != value*1000) {
                    _checkTime = value*1000;
                    OnPropertyChanged("CheckTime");
                }
            }
        }
        #endregion CheckTime

        #region IsStopped
        private bool _isStopped;

        public bool IsStopped { get { return _isStopped; } }
        #endregion IsStopped

        private bool _disposed;
        #endregion Properties

        public VpnTool(string vpnName, string userName, string password, int checkTimeSec = 60) {
            _disposed = false;
            _isStopped = false;
            VpnName = vpnName;
            UserName = userName;
            Password = password;
            CheckTime = checkTimeSec;
            BackWorker = new BackgroundWorker {WorkerSupportsCancellation = true};
            BackWorker.DoWork += BackWorkerDoWork;
            BackWorker.RunWorkerCompleted += BackWorkerRunWorkerCompleted;
        }

        public void Start() {
            if (BackWorker.IsBusy != true) {
                BackWorker.RunWorkerAsync();
            }
        }

        public void Stop() {
            if (BackWorker.WorkerSupportsCancellation) {
                _isStopped = true;
                BackWorker.CancelAsync();     
            }
        }

        private void BackWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            if ((e.Cancelled)) {
                _isStopped = true;
            } else if (e.Error != null) {
                LastErrorMessage = e.Error.Message;
            }
        }

        private void BackWorkerDoWork(object sender, DoWorkEventArgs e) {
            while (!_isStopped && !e.Cancel) {
                IsConnected = CheckVpn(VpnName);
                if (!IsConnected)
                    ConnectToVpn(VpnName, UserName, Password);
                Sleeper();                
            }
        }

        private bool CheckVpn(string vpnName) {
            if (NetworkInterface.GetIsNetworkAvailable()) {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                return
                    interfaces.Where(Interface => Interface.OperationalStatus == OperationalStatus.Up).Where(
                        Interface =>
                        (Interface.NetworkInterfaceType == NetworkInterfaceType.Ppp) &&
                        (Interface.NetworkInterfaceType != NetworkInterfaceType.Loopback)).Any(
                            Interface => Interface.Name.Trim().ToLower() == vpnName.Trim().ToLower());
            }
            return false;
        }

        private void ConnectToVpn(string vpnname, string username, string password) {
            try {
                System.Diagnostics.Process.Start("rasdial.exe", Enqouted(vpnname)
                                                                + " " + Enqouted(username)
                                                                + " " + Enqouted(password));
            } catch (Exception ex) {
                LastErrorMessage = ex.Message;
            }
        }

        private static string Enqouted(string value) { return '"' + value.Trim() + '"'; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string status) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(status));
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    VpnName = null;
                    UserName = null;
                    Password = null;
                    PropertyChanged = null;
                    BackWorker = null;
                }
                _disposed = true;
            }
        }

        private void Sleeper() {
            int sleepTime = CheckTime / 1000;
            for (int i = 0; i < sleepTime; i++)
            {
                if(IsStopped)
                    return;
                Thread.Sleep(1000);   
            }            
        }
    }
}