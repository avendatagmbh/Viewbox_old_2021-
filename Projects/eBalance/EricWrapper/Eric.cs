using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using eBalanceKitBusiness.Structures;
using System.Diagnostics;

namespace EricWrapper {

    public class Eric {

        public bool IsTest { get; set; }

        public event EventHandler Finished;
        private void OnFinished() { if (this.Finished != null) Finished(this, new EventArgs()); }

        enum eric_bearbeitung_flag_t{
            ERIC_VALIDIERE = 1 << 1,
            ERIC_SEND_NO_SIG = 1 << 2,
            ERIC_SEND_AUTH = 1 << 4,
            ERIC_PRINT_PDF = 1 << 5
        };
        
        [DllImport("ERIC\\ericapi.dll")]
        private static extern int EricEinstellungSetzen(string name, string value);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        class eric_druck_parameter_t {
            public int vorschau; // = 1;
            public int ersteSeite; // = 1;
            [MarshalAs(UnmanagedType.LPStr)] public string fussText;
            [MarshalAs(UnmanagedType.LPStr)] public string pdfName;
        }
       
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        class eric_verschluesselungs_parameter_t {
            [MarshalAs(UnmanagedType.U4)] public uint zertifikatHandle;
            [MarshalAs(UnmanagedType.LPStr)] public string pin;
        }

        [DllImport("ERIC\\ericapi.dll")]
        private static extern int EricFinanzamtNo(
            int ArtDerDatenuebergabe,
            string InfoType,
            StringBuilder ListBuffer);


        [DllImport("ERIC\\ericapi.dll")]
        private static extern int EricGetHandleToCertificate(
            ref uint hToken,
            ref int iInfo_Pin_Required,
            string path_to_keystore);

        [DllImport("ERIC\\ericapi.dll")]
        private static extern int EricCloseHandleToCertificate(uint hToken);

        [DllImport("ERIC\\ericapi.dll")]
        private static extern int EricHoleFehlerText(
            int fehlerkode,
            StringBuilder klartextFehler,
            ref int klartextFehlerGroesse);
        
        [DllImport("ERIC\\ericapi.dll")]
        private static extern int EricBearbeiteVorgang(
            byte[] datenpuffer,
            int bearbeitungsFlags,
            StringBuilder rueckgabePuffer,
            ref long laengeRueckgabePuffer,
            [MarshalAs(UnmanagedType.LPStruct)] eric_druck_parameter_t druckParameter,
            [MarshalAs(UnmanagedType.LPStruct)] eric_verschluesselungs_parameter_t cryptoParameter);

        [DllImport("ERIC\\ericapi.dll")]
        private static extern int EricPruefeSteuernummer(
                string steuernummer,
                StringBuilder pruefzifferVorSteuernummerumstellung, //reserve at least 100 chars
                StringBuilder pruefzifferNachSteuernummerumstellung	 //same
        );

        [DllImport("ERIC\\ericapi.dll")]
        private static extern int EricCheckBuFaNummer(
            string cSteuernummer
            );

        [DllImport("ERIC\\ericapi.dll")]
        private static extern int EricPruefeIdentifikationsMerkmal(
            string SteuerId,
            StringBuilder returnBuffer	 
            );

        public static void Init() {
            string homeDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ERIC";

            string localDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AvenDATA\\eBalanceKit\\ERIC";
            if (!Directory.Exists(localDataDir)) Directory.CreateDirectory(localDataDir);

            string dataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\AvenDATA\\eBalanceKit\\ERIC";
            if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);

            string logDir = localDataDir + "\\log";
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

            string tempDir = localDataDir + "\\temp";
            if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

            int result;
            result = EricEinstellungSetzen("basis.home_dir", homeDir);
            result = EricEinstellungSetzen("basis.data_dir", localDataDir);
            //result = EricEinstellungSetzen("basis.data_dir", dataDir); (INI_HOME - entfällt)
            result = EricEinstellungSetzen("basis.log_dir", logDir);
            result = EricEinstellungSetzen("basis.temp_dir", tempDir);

            //EricSetInitDefaults();

            //try {
            //    //Process p = new Process();
            //    //ProcessStartInfo pi = new ProcessStartInfo("ERIC\\ericSystemCheck.exe");
            //    //p.StartInfo = pi;
            //    //p.Start();
            //} catch (Exception ex) {

            //}

            //StringBuilder sb = new StringBuilder(1024*1024*20);
            //int x = EricFinanzamtNo(4096, "L", sb);
        }

        public static void SetProxy(string host, string port, string username = null, string password = null) {
            EricEinstellungSetzen("http.proxy_host", host);
            if (!string.IsNullOrEmpty(port)) EricEinstellungSetzen("http.proxy_port", port);
            if (!string.IsNullOrEmpty(username)) EricEinstellungSetzen("http.proxy_username", username);
            if (!string.IsNullOrEmpty(password)) EricEinstellungSetzen("http.proxy_password", password);
        }

        public void Validate(StringBuilder data) {
            try {
                //var x = data.ToString();
                //System.IO.File.WriteAllText("validationCheck.xbrl", data.ToString());

                long len = 40000;
                StringBuilder bufferVerify = new StringBuilder((int)len);
                
                // DEBUG
                //string x = data.ToString();
                //StreamWriter w = new StreamWriter("tmp.xml");
                //w.Write(x);
                //w.Close();
                //StreamReader r = new StreamReader("tmp.xml");
                //x = r.ReadToEnd();
                //r.Close();
                //byte[] dataArray = Encoding.GetEncoding("iso-8859-1").GetBytes(x);

                byte[] dataArray = Encoding.GetEncoding("iso-8859-1").GetBytes(data.ToString());
                int rc = EricBearbeiteVorgang(
                     dataArray,
                     34,
                     bufferVerify,
                     ref len,
                     null,
                     null);

                int err1n = 4000;
                StringBuilder err1 = new StringBuilder(4000);
                EricHoleFehlerText(rc, err1, ref err1n);

                this.ResultMessage = bufferVerify.ToString();
                
                if (string.IsNullOrEmpty(ResultMessage)) throw new Exception(err1.ToString());
                else ExtractResultMessages();

                this.VerificationSucceed = (rc == 0);

            } catch (Exception ex) {
                this.LastError = ex.Message;
                this.UnknownErrorOccured = true;

            } finally {
                OnFinished();
            }
        }

        public void SendData(string pdfFilename, StringBuilder data, string certFile, string pin) {
            try {
                this.UnknownErrorOccured = false;
                this.SendSucceed = false;
                byte[] dataArray = Encoding.GetEncoding("iso-8859-1").GetBytes(data.ToString());

                #region verify data
                {
                    long len = 40000;
                    StringBuilder buffer = new StringBuilder((int)len);

                    eric_druck_parameter_t printParams = new eric_druck_parameter_t();
                    printParams.vorschau = 0;
                    printParams.ersteSeite = 0;
                    printParams.pdfName = pdfFilename;
                    printParams.fussText = null;

                    int rc = EricBearbeiteVorgang(dataArray,
                        (int)eric_bearbeitung_flag_t.ERIC_VALIDIERE |
                        (int)eric_bearbeitung_flag_t.ERIC_PRINT_PDF,
                        buffer, ref len, printParams, null);

                    this.VerificationSucceed = (rc == 0);
                    if (this.VerificationSucceed) {
                        this.ResultMessage = buffer.ToString();
                    } else {
                        int errSize = 40000;
                        StringBuilder errorText = new StringBuilder(errSize);
                        EricHoleFehlerText(rc, errorText, ref errSize);
                        LastError = errorText.ToString();
                        //UnknownErrorOccured = true;
                        ResultMessage = buffer.ToString();
                        if (string.IsNullOrEmpty(ResultMessage)) throw new Exception(errorText.ToString());
                        else ExtractResultMessages();
                        return;
                    }
                }
                #endregion verify data

                #region send data
                {
                    long len = 40000;
                    StringBuilder buffer = new StringBuilder((int)len); 
                    
                    // init crypto parameters
                    uint hToken = 0;
                    int iInfo_Pin_Required = 0;
                    int rcGetCertHandle = EricGetHandleToCertificate(ref hToken, ref iInfo_Pin_Required, certFile);
                    eric_verschluesselungs_parameter_t cryptoParameter = new eric_verschluesselungs_parameter_t();
                    cryptoParameter.zertifikatHandle = hToken;
                    cryptoParameter.pin = pin;

                    // handle error in crypto parameter initialisation
                    if (rcGetCertHandle != 0) {
                        int errSize = 40000;
                        StringBuilder errorText = new StringBuilder(errSize);
                        EricHoleFehlerText(rcGetCertHandle, errorText, ref errSize);
                        LastError = errorText.ToString();
                        UnknownErrorOccured = true;

                        this.ResultMessage = buffer.ToString();

                        if (string.IsNullOrEmpty(ResultMessage)) throw new Exception(errorText.ToString());
                        else ExtractResultMessages();
                    }

                    eric_druck_parameter_t printParams = new eric_druck_parameter_t();
                    printParams.vorschau = 0;
                    printParams.ersteSeite = 0;
                    printParams.pdfName = pdfFilename;
                    printParams.fussText = null;

                    int rc = EricBearbeiteVorgang(dataArray,
                        (int)eric_bearbeitung_flag_t.ERIC_VALIDIERE | 
                        (int)eric_bearbeitung_flag_t.ERIC_PRINT_PDF | 
                        (int)eric_bearbeitung_flag_t.ERIC_SEND_AUTH,
                        buffer,ref len, printParams, cryptoParameter);

                    int rcCloseCertHandle = EricCloseHandleToCertificate(hToken);

                    this.SendSucceed = (rc == 0);
                    if (this.SendSucceed) {
                        this.ResultMessage = buffer.ToString();
                    } else {
                        int errSize = 40000;
                        StringBuilder errorText = new StringBuilder(errSize);
                        EricHoleFehlerText(rc, errorText, ref errSize);
                        LastError = errorText.ToString();
                        //UnknownErrorOccured = true;
                        ResultMessage = buffer.ToString();
                        if (string.IsNullOrEmpty(ResultMessage)) throw new Exception(errorText.ToString());
                        else ExtractResultMessages();
                    }
                }
                #endregion send data

            } catch (Exception ex) {
                this.LastError = ex.Message;
                this.UnknownErrorOccured = true;

            } finally {
                if (ResultMessages == null) ResultMessages = new List<EricResultMessage>();
                OnFinished();
            }
        }

        private void ExtractResultMessages() {
            ResultMessages = new List<EricResultMessage>();

            string[] messageParts = this.ResultMessage.Split('|');
            ResultMessages.Clear();
            int pos = 0;
            foreach (string tmp in messageParts) {
                if (pos == 0) ResultMessages.Add(new EricResultMessage());
                switch (pos) {
                    case 0:
                        ResultMessages.Last().Id = tmp;
                        break;

                    case 1:
                        ResultMessages.Last().FK = tmp;
                        break;

                    case 2:
                        ResultMessages.Last().USB = tmp;
                        break;

                    case 3:
                        ResultMessages.Last().MZI = tmp;
                        break;

                    case 4:
                        ResultMessages.Last().VD = tmp;
                        break;

                    case 5:
                        ResultMessages.Last().PK = tmp;
                        break;

                    case 6:
                        ResultMessages.Last().RId = tmp;
                        break;

                    case 7:
                        ResultMessages.Last().EId = tmp;
                        break;

                    case 8:
                        ResultMessages.Last().Text = tmp;
                        break;
                }
                pos++; pos %= 9;
            }
        }

        private bool ErrorCheck(int rc) {
            if (rc == 0) return true;
            int errSize = 40000;
            StringBuilder errorText = new StringBuilder(errSize);
            Eric.EricHoleFehlerText(rc, errorText, ref errSize);
            LastError = errorText.ToString();
            return false;
        }

        public bool PruefeSteuernummer(string nr) {
            StringBuilder result1 = new StringBuilder(101), result2 = new StringBuilder(101);

            int rc = Eric.EricPruefeSteuernummer(nr, result1, result2);
            return ErrorCheck(rc);
        }

        public bool PruefeBufa(string nr) {
            int rc = Eric.EricCheckBuFaNummer(nr);
            return ErrorCheck(rc);
        }

        public bool PruefeIdentifikationsNr(string nr) {
            StringBuilder result = new StringBuilder(151);
            int rc = Eric.EricPruefeIdentifikationsMerkmal(nr, result);
            return ErrorCheck(rc);
        }

        public bool UnknownErrorOccured { get; private set; }
        public string LastError { get; private set; }
        public bool VerificationSucceed { get; private set; }
        public bool SendSucceed { get; private set; }
        public string ResultMessage { get; private set; }
        public List<EricResultMessage> ResultMessages { get; set; }
    }
}
