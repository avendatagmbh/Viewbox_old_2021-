// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-06-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using EricWrapper;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Structures;

namespace EricTest {
    public class Model :Utils.NotifyPropertyChangedBase {

        public Model(Window owner) { Owner = owner; }

        private DlgProgress Progress { get; set; }
        private Window Owner { get; set; }

        #region XbrlContent
        private string _xbrlContent;

        public string XbrlContent {
            get { return _xbrlContent; }
            set {
                if (_xbrlContent != value) {
                    _xbrlContent = value;
                    OnPropertyChanged("XbrlContent");
                }
            }
        }
        #endregion XbrlContent

        #region XbrlFile
        private string _xbrlFile;

        public string XbrlFile {
            get { return _xbrlFile; }
            set {
                if (_xbrlFile != value) {
                    _xbrlFile = value;
                    OnPropertyChanged("XbrlFile");
                    if(File.Exists(value)) {
                        SetContent();
                    }
                }
            }
        }
        #endregion XbrlFile

        public void Validate() {
            Progress = new DlgProgress(Owner);
            Progress.ProgressInfo.Caption = "validate";
            Progress.ProgressInfo.IsIndeterminate = true;
            new Thread(StartEricValidation) { CurrentCulture = Thread.CurrentThread.CurrentCulture, CurrentUICulture = Thread.CurrentThread.CurrentUICulture }.Start();
            Progress.ShowDialog();
        }

        private void SetContent() {
            using (StreamReader sr = new StreamReader(XbrlFile)) {
                XbrlContent = sr.ReadToEnd();
            }
        }


        private void StartEricValidation() {
            while (!Progress.IsVisible) Thread.Sleep(10);
            var eric = new Eric();
            eric.Finished += eric_Finished;
            eric.Validate(new StringBuilder(XbrlContent));
        }


        private void eric_Finished(object sender, System.EventArgs e) {
            var eric = sender as Eric;
            Owner.Dispatcher.Invoke(
                DispatcherPriority.Background,
                new Action(delegate {
                if (eric.UnknownErrorOccured) {
                    MessageBox.Show(eric.LastError, "Unbekannter Fehler bei der Plausibilisierung",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                    Progress.Close();
                    Progress = null;
                }
                else {
                    if (!eric.VerificationSucceed) {
                        string msg =
                            "Die eingegebenen Daten sind unvollständig oder ungültig. Bitte prüfen Sie Ihre Eingaben.";
                        int i = 1;
                        foreach (EricResultMessage message in eric.ResultMessages) {
                            msg += Environment.NewLine + Environment.NewLine + i + ". " + message.Text;
                            i++;
                        }

                        MessageBox.Show(msg, "Fehler bei der Plausibilisierung", MessageBoxButton.OK,
                                        MessageBoxImage.Error);
                        Progress.Close();
                        Progress = null;
                    }
                    else {
                        MessageBox.Show(
                            "Bei der Plausibilisierung der Daten konnten keine Probleme gefunden werden.",
                            "Plausibilisierung erfolgreich", MessageBoxButton.OK, MessageBoxImage.Information);
                        Progress.Close();
                        Progress = null;
                    }
                }
            }));
        } 
    }
}