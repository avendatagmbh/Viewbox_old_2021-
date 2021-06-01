// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-02-29
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;
using EricWrapper;
using eBalanceKitBase.Structures;
using eBalanceKitBusiness.Exceptions;
using eBalanceKitBusiness.Export;
using eBalanceKitBusiness.Logs;
using eBalanceKitBusiness.Logs.DbMapping;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness {
    public class EricController {

        #region Error
        public event EventHandler<MessageEventArgs> Error;
        private void OnError(string message, string caption) {
            if (Error != null) Error(this, new MessageEventArgs(message, caption));
            Log(message);
        }
        #endregion Error
        
        #region Info
        public event EventHandler<MessageEventArgs> Info;
        private void OnInfo(string message, string caption) {
            if (Info != null) Error(this, new MessageEventArgs(message, caption));
            Log(message);
        }
        #endregion Info

        private StringBuilder _lastSendData;
        private readonly object _lockObj = new object();
        private readonly string _logfileName;

        public EricController(ProgressInfo progressInfo, Document document, string password, string certFile, bool isTest) {
            _logfileName =
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                "\\AvenDATA\\eBalanceKit\\eBalanceKit_" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            
            ProgressInfo = progressInfo;
            Document = document;
            Password = password;
            CertFile = certFile;
            IsTest = isTest;
        }

        private ProgressInfo ProgressInfo { get; set; }
        private Document Document { get; set; }
        private string Password { get; set; }
        private string CertFile { get; set; }
        private bool IsTest { get; set; }

        private void Log(string message) {
            lock (_lockObj) {
                var w = new StreamWriter(_logfileName, true);
                w.WriteLine("<" + DateTime.Now + "> " + message);
                w.Close();
            }
        }

        private void Log1(string message) {
            lock (_lockObj) {
                var w = new StreamWriter(_logfileName, true);
                w.WriteLine(message);
                w.Close();
            }
        }

        public void Start() {
            var caption = (IsTest ? "Testübertragung" : "Datenübertragung");

            Log1("--------------------------------------------------------------------------------");
            Log1(Document.Company.Name + " / Geschäftsjahr " + Document.FinancialYear.FYear + " / " + Document.Name);
            Log1("--------------------------------------------------------------------------------");
            Log(caption + " gestartet.");

            try {
                var eric = new Eric { IsTest = IsTest };
                eric.Finished += EricOnFinished;

                //this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new voidDelegate(delegate {
                //    MessageBox.Show("Starte " + caption + ".");
                //}));

                string pdfFileName = Document.Company.Name + "_" + Document.FinancialYear.FYear + "_" + Document.Name +
                                     ".pdf";
                _lastSendData = XbrlExporter.GetElsterXml(Document, IsTest);
                eric.SendData(pdfFileName, _lastSendData, CertFile, Password);
            } catch (Exception ex) {
                OnError("Unerwarteter Fehler: " + ex.Message, "Unerwarteter Fehler");
            }
        }

        private void EricOnFinished(object sender, System.EventArgs eventArgs) {
            var eric = sender as Eric;
            string caption = (eric.IsTest ? "Testübertragung" : "Datenübertragung");

            Log(caption + " abgeschlossen.");
            try {
                    try {
                        //MessageBox.Show(caption + " abgeschlossen...");

                        //MessageBox.Show("Verifizierung erfolgreich: " + eric.VerificationSucceed + " | Senden erfolgreich: " + eric.SendSucceed +
                        //    " | ResultMessages : " + (eric.ResultMessages == null ? "NULL" : "OK"));

                        if (eric.UnknownErrorOccured) {
                            Log1("Unbekannter Fehler: " + eric.LastError);
                            if (!eric.IsTest)
                                LogManager.Instance.AddSendLog(_lastSendData.ToString(),
                                                               DbSendLog.SendErrorType.UnknownError,
                                                               eric.LastError, Document.Id);

                            OnError(eric.LastError, "Unbekannter Fehler");
                        } else if (!eric.VerificationSucceed) {
                            string msg =
                                "Die eingegebenen Daten sind unvollständig oder ungültig. Bitte prüfen Sie Ihre Eingaben.";
                            int i = 1;
                            foreach (EricResultMessage message in eric.ResultMessages) {
                                msg += Environment.NewLine + Environment.NewLine + i + ". " +
                                       message.Text;
                                i++;
                            }


                            if (msg.Contains("eine nicht unterstützte Ressource angefordert")) {

                                msg = "Sehr geehrter Anwender," +
                                      Environment.NewLine +
                                      Environment.NewLine +
                                      "Von der Finanzbehörde wird ein so genannter Elster Rich Client (kurz: ERIC) zur Verfügung gestellt, welcher vor dem Senden des Berichts eine Überprüfung der zu übermittelnden Informationen vornimmt. " +
                                      Environment.NewLine +
                                      "Leider ist der aktuell verfügbare ERIC noch nicht an die neuste Taxonomie angepasst und stößt somit auf Probleme. Die Finanzverwaltung wird im November 2012 ein Update für den ERIC bereitstellen, der mit der Taxonomie 5.1 kompatibel ist. Dieses Update wird dann umgehend in das eBilanz-Kit integriert, bis dahin ist aus technischen Gründen kein Testversand möglich.";
                            }


                            if (!eric.IsTest)
                                LogManager.Instance.AddSendLog(_lastSendData.ToString(),
                                                               DbSendLog.SendErrorType.
                                                                   VerificationError, msg,
                                                               Document.Id);

                            Log1("Fehler bei der Plausibilisierung: " + msg);
                            OnError(msg, "Fehler bei der Plausibilisierung");

                        } else if (!eric.SendSucceed) {
                            Log1("Fehler beim Senden: " + eric.LastError);
                            if (!eric.IsTest)
                                LogManager.Instance.AddSendLog(_lastSendData.ToString(),
                                                               DbSendLog.SendErrorType.SendError,
                                                               eric.LastError, Document.Id);

                            OnError(eric.LastError, "Fehler beim Senden");
                        } else {
                            if (!eric.IsTest)
                                LogManager.Instance.AddSendLog(_lastSendData.ToString(),
                                                               DbSendLog.SendErrorType.NoError, "",
                                                               Document.Id);
                            Log1(caption + " erfolgreich.");
                            Log1(eric.ResultMessage + " erfolgreich!");

                            string msg =
                                caption + " erfolgreich." +
                                (eric.ResultMessage.Contains("Daten erfolgreich an EPoS uebergeben")
                                     ? Environment.NewLine +
                                       "Daten erfolgreich an EPoS uebergeben"
                                     : "");

                            OnInfo(msg, caption + " erfolgreich");
                        }
                    } catch (Exception ex) {
                        try {
                            if (!eric.IsTest)
                                LogManager.Instance.AddSendLog(_lastSendData.ToString(),
                                                               DbSendLog.SendErrorType.UnknownError,
                                                               ex.Message, Document.Id);
                            Log1("Unerwarteter Fehler: " + ex.Message);
                            OnError("Unerwarteter Fehler: " + ex.Message, "Unerwarteter Fehler");
                        } catch (Exception) { }
                    }
            } catch (Exception ex) {
                Log1("Unerwartetere Fehler: " + ex.Message);
            } finally {
                Log1("--------------------------------------------------------------------------------");
                Log1("");
                Log1("");
                Log1("");
            }
            
        }
    }
}