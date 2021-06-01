using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utils;
using WpfControlsSample.Windows;

namespace WpfControlsSample.Models {
    public class ButtonsDemoModel : NotifyPropertyChangedBase{

        #region Constructor
        internal ButtonsDemoModel(Window owner) {
            _owner = owner;
        }
        #endregion Constructor

        #region Properties
        private Window _owner;

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
        
        #endregion Properties

        #region Methods
        public void CreateProgress() {
            ProgressCalculator progress = new ProgressCalculator() { WorkerSupportsCancellation = true };
            progress.DoWork += progress_DoWork;
            progress.RunWorkerCompleted += progress_RunWorkerCompleted;
            DlgProgress progressDlg = new DlgProgress() { DataContext = progress, Owner = _owner };
            progress.RunWorkerAsync();
            progressDlg.ShowDialog();
            
        }
        void progress_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) {
            if (e.Error != null)
                MessageBox.Show(_owner, "Error: " + e.Error.Message);
            else
                MessageBox.Show(_owner, "Progress finished");

        }

        void progress_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e) {
            const int steps = 50;
            ProgressCalculator progress = sender as ProgressCalculator;
            progress.Title = "Title";
            progress.SetWorkSteps(steps, false);
            for (int i = 0; i < steps; ++i) {
                if (progress.CancellationPending)
                    throw new TaskCanceledException("Canceled task");
                progress.Description = "Performing Step " + (i + 1);
                progress.StepDone();
                Thread.Sleep(100);
            }
        }
        #endregion Methods
    }
}
