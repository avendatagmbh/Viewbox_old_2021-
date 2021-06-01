using System;
using System.ComponentModel;
using System.Windows;
using DatabaseExporter.Windows;
using Utils;

namespace RtfConverterWpf.Structures {
    public class ProgressDialogHelper {
        #region Constructor
        public ProgressDialogHelper(Window owner) {
            _owner = owner;
            ProgressCalculator = new ProgressCalculator();
            ProgressCalculator.DoWork += ProgressCalculator_DoWork;
            ProgressCalculator.RunWorkerCompleted += ProgressCalculator_RunWorkerCompleted;

            ErrorMessage = "Es ist folgender Fehler aufgetreten: " + Environment.NewLine + "{0}";
            CanceledMessage = "Der Job wurde abgebrochen";
        }


        #endregion Constructor


        #region Properties
        public ProgressCalculator ProgressCalculator { get; private set; }
        private PopupProgressBar _dlg;
        public string CanceledMessage { get; set; }
        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        private readonly Window _owner;
        #endregion Properties

        #region Methods
        public void StartJob(Action<ProgressCalculator> func) {
            _dlg = new PopupProgressBar() { DataContext = ProgressCalculator, Owner = _owner };
            ProgressCalculator.RunWorkerAsync(func);
            _dlg.ShowDialog();
        }

        public void StartJob(Action<ProgressCalculator, DoWorkEventArgs> func) {
            _dlg = new PopupProgressBar() { DataContext = ProgressCalculator, Owner = _owner };
            ProgressCalculator.RunWorkerAsync(func);
            _dlg.ShowDialog();
        }

        void ProgressCalculator_DoWork(object sender, DoWorkEventArgs e) {
            Action<ProgressCalculator> func = e.Argument as Action<ProgressCalculator>;
            if(func != null)
                func(ProgressCalculator);
            else {
                Action<ProgressCalculator, DoWorkEventArgs> func2 = e.Argument as Action<ProgressCalculator, DoWorkEventArgs>;
                func2(ProgressCalculator, e);
            }
        }

        void ProgressCalculator_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e) {
            if (e.Cancelled) {
                MessageBox.Show(_dlg, CanceledMessage, "", MessageBoxButton.OK, MessageBoxImage.Information);
            } else if (e.Error != null) {
                MessageBox.Show(_dlg, string.Format(ErrorMessage, e.Error.Message), "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if(SuccessMessage != null)
                MessageBox.Show(_dlg, SuccessMessage, "", MessageBoxButton.OK, MessageBoxImage.Information);
            _dlg.Close();
        }
        #endregion Methods
    }
}
