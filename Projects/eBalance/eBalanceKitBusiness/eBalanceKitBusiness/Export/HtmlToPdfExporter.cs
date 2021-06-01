// --------------------------------------------------------------------------------
// author: Taba Ernő
// since:  2012-11-26
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Utils;
using System.IO;
using Microsoft.Win32;
using System.Windows;
using eBalanceKitResources.Localisation;
using System;
using Microsoft.Office.Interop.Word;

namespace eBalanceKitBusiness.Export
{
    public class HtmlToPdfExporter : NotifyPropertyChangedBase {

        private string _htmlstring;
        private WdOrientation _orientation;
        private float _margintop;
        private float _marginbottom;
        private float _marginleft;
        private float _marginright;

        /// <summary>
        /// Converts the HTML to PDF.
        /// </summary>
        /// <param name="pHtmlString">The HTML string.</param>
        /// <param name="pDefaultFileName">Name of the default file.</param>
        /// <param name="pIsLandscape">if set to <c>true</c> [is landscape].</param>
        /// <param name="pmargintop">The pmargintop.</param>
        /// <param name="pmarginbottom">The pmarginbottom.</param>
        /// <param name="pmarginleft">The pmarginleft.</param>
        /// <param name="pmarginright">The pmarginright.</param>
        public void ConvertHtmlToPdf(
            string pHtmlString, 
            string pDefaultFileName, 
            bool pIsLandscape,
            float pmargintop,
            float pmarginbottom,
            float pmarginleft,
            float pmarginright)
        {         
            if (string.IsNullOrEmpty(pHtmlString))
                return;

            if (!IsOfficeInstalled()) {
                MessageBox.Show("For export nessessery installed MS Word");
                return;
            }

            _orientation = pIsLandscape ? WdOrientation.wdOrientLandscape : WdOrientation.wdOrientPortrait;

            _htmlstring = HtmlWidthParser(pHtmlString);
            _margintop = pmargintop;
            _marginbottom = pmarginbottom;
            _marginleft = pmarginleft;
            _marginright = pmarginright;

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileOk += DlgFileOk;
            dlg.Filter = "PDF files (*.pdf)|*.pdf";
            dlg.FileName = pDefaultFileName;
            dlg.ShowDialog();
        }

        /// <summary>
        /// Determines whether [is office installed].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is office installed]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsOfficeInstalled()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\Winword.exe");
            if (key != null)
            {
                key.Close();
            }
            return key != null;
        }

        /// <summary>
        /// Writes the HTML to file.
        /// </summary>
        /// <param name="pHtmlPath">The HTML path.</param>
        /// <returns></returns>
        private bool WriteHtmlToFile(string pHtmlPath) {
            using (StreamWriter sw = new StreamWriter(pHtmlPath, true, Encoding.Default))
            {
                sw.Write(_htmlstring);
                return true;
            }
        }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="pPath">The path.</param>
        private void deleteFile(string pPath) {
            if(File.Exists(pPath))
                File.Delete(pPath);
        }

        /// <summary>
        /// DLGs the file ok.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see>
        ///                       <cref>CancelEventArgs</cref>
        ///                     </see> instance containing the event data.</param>
        void DlgFileOk(object sender, CancelEventArgs e) {
            try
            {
                SaveFileDialog dlg = (sender as SaveFileDialog);
                if(dlg == null || dlg.FileName == null)
                    return;

                object htmlFilePath = dlg.FileName.ToLower().Replace("pdf", "html");
                object docFilePath = dlg.FileName.ToLower().Replace("pdf", "doc");
                object pdfFilePath = dlg.FileName;
                object oMissing = System.Reflection.Missing.Value;
                object oFalse = false;
                object fileFormat = WdSaveFormat.wdFormatPDF;
                fileFormat = WdSaveFormat.wdFormatDocument;

                if (!WriteHtmlToFile(htmlFilePath.ToString()))
                    return;

                Microsoft.Office.Interop.Word.Application oWord = new Microsoft.Office.Interop.Word.Application{Visible = false};
                Document oDoc = oWord.Documents.Open(ref htmlFilePath, ref oMissing,
                                                     ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                                     ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                                                     ref oMissing, ref oMissing, ref oMissing, ref oMissing);

                oDoc.PageSetup.PaperSize = WdPaperSize.wdPaperA4;
                oDoc.PageSetup.TopMargin = _margintop;
                oDoc.PageSetup.BottomMargin = _marginbottom;
                oDoc.PageSetup.LeftMargin = _marginleft;
                oDoc.PageSetup.RightMargin = _marginright;
                oDoc.PageSetup.LayoutMode = WdLayoutMode.wdLayoutModeDefault;
                oDoc.PageSetup.Orientation = _orientation;

                object oEndings = WdLineEndingType.wdCRLF;
                oDoc.SaveAs(ref docFilePath, ref fileFormat, ref oMissing,
                            ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                            ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                            ref oMissing, ref oEndings, ref oMissing);

                oDoc.Close(ref oFalse, ref oMissing, ref oMissing);
                oWord.Quit(ref oMissing, ref oMissing, ref oMissing);

                //Delete temp html file:
                deleteFile(htmlFilePath.ToString());

                // Open the file:
                string fileName = ((SaveFileDialog)sender).FileName;
                fileName = docFilePath.ToString();
                if (MessageBox.Show(ResourcesCommon.FileSaveSuccessfulOpen, ResourcesCommon.FileSaved, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    using (System.Diagnostics.Process.Start(fileName)) { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString(CultureInfo.InvariantCulture));
            }            

        }

        private string HtmlWidthParser(string pHtml) {
            string pattern = "width=";
            int maxWidth = 200;
            string[] splitter = new string[1] { pattern };
            string[] str = pHtml.Split(splitter, StringSplitOptions.RemoveEmptyEntries);
            int startPos = 0;

            //At the 0. position no needed:
            for (int i = 1; i < str.Length; i++ ) {
                startPos = pHtml.IndexOf(pattern, startPos, StringComparison.OrdinalIgnoreCase) + pattern.Length;
                string res = pHtml.Substring(startPos, 10);
                Tuple<int, int> szamok = ConvertStringToInt(res);

                if (maxWidth < szamok.Item1)
                {
                    pHtml = pHtml.Remove(startPos+szamok.Item2, szamok.Item1.ToString().Length);
                    pHtml = pHtml.Insert(startPos + szamok.Item2, maxWidth.ToString());
                }                
            }

                //if(!String.IsNullOrEmpty(pHtml)) {
                //    if (pHtml.Contains(pattern)) {
                //        int startPos = pHtml.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) + pattern.Length;
                //        string res = pHtml.Substring(startPos, 10);
                //        int szam = ConvertStringToInt(res);
                //        if(maxSzelesseg < szam) {
                //            pHtml = pHtml.Remove(startPos, szam.ToString().Length);
                //            pHtml = pHtml.Insert(startPos, maxSzelesseg.ToString());
                //        }


                //    }
                //}

                return pHtml;
        }

        private Tuple<int, int> ConvertStringToInt(string pStr) { 
            char[] chrArr = pStr.ToCharArray();
            string res = String.Empty;
            int corrigateInt = 0;

            foreach (char c in chrArr) {
                if(Char.IsNumber(c)) {
                    res += c;
                } else {
                    if (String.IsNullOrEmpty(res)) {
                        corrigateInt++;
                        continue;
                    } else
                        break;    
                }                
            }

            return new Tuple<int, int>(Convert.ToInt32(res), corrigateInt);
        }

    }
}
