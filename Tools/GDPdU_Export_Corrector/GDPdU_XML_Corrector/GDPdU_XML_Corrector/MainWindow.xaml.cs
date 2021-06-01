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
using System.IO;
using Microsoft.Win32;

namespace GDPdU_XML_Corrector {
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "*.xml|*.xml";
            bool? result = dlg.ShowDialog(); 
            if (result.HasValue && result.Value == true) {
                txtPath.Text = dlg.FileName;
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {
            
            bool errOccured = false;
            bool foundInvalidChar = false;

            string allowedChars = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZöäüÖÄÜ-_<><1234567890ß!""§$%&/()=?/+*`´'^ .";

            if (!File.Exists(txtPath.Text)) {
                MessageBox.Show("File not found.");
            }

            File.Move(txtPath.Text, txtPath.Text + ".bak");

            StreamReader reader = new StreamReader(txtPath.Text + ".bak", Encoding.UTF7);
            StreamWriter writer = new StreamWriter(txtPath.Text);

            try {
                while (!reader.EndOfStream) {
                    string line = reader.ReadLine();
                    string tmp = line;
                    foreach (char ch in line.ToCharArray()) {
                        if (!allowedChars.Contains(ch)) {
                            switch ((int)ch) {
                                case 192: // À
                                case 193: // Á
                                case 194: // Â
                                case 195: // Ã
                                case 196: // Ä
                                case 197: // Å
                                    tmp = tmp.Replace(ch, 'Ä');
                                    break;

                                case 198: // Æ
                                    tmp = tmp.Replace(ch.ToString(), "AE");
                                    break;

                                case 199: // Ç
                                    tmp = tmp.Replace(ch.ToString(), "C");
                                    break;

                                case 200: // È
                                case 201: // É
                                case 202: // Ê
                                case 203: // Ë
                                    tmp = tmp.Replace(ch.ToString(), "E");
                                    break;

                                case 204: // Ì
                                case 205: // Í
                                case 206: // Î
                                case 207: // Ï
                                    tmp = tmp.Replace(ch.ToString(), "I");
                                    break;

                                case 208: // Ð
                                    tmp = tmp.Replace(ch.ToString(), "D");
                                    break;

                                case 209: // Ñ
                                    tmp = tmp.Replace(ch.ToString(), "N");
                                    break;

                                case 210: // Ò
                                case 211: // Ó
                                case 212: // Ô
                                case 213: // Õ
                                case 214: // Ö
                                    tmp = tmp.Replace(ch.ToString(), "Ö");
                                    break;

                                case 217: // Ù
                                case 218: // Ú
                                case 219: // Û
                                case 220: // Ü
                                    tmp = tmp.Replace(ch.ToString(), "Ü");
                                    break;

                                case 221: // Ý
                                    tmp = tmp.Replace(ch.ToString(), "Y");
                                    break;

                                case 224: // à	
                                case 225: // á
                                case 226: // â
                                case 227: // ã
                                case 228: // ä
                                case 229: // å
                                    tmp = tmp.Replace(ch, 'ä');
                                    break;

                                case 230: // æ
                                    tmp = tmp.Replace(ch.ToString(), "ae");
                                    break;

                                case 231: // ç
                                    tmp = tmp.Replace(ch, 'c');
                                    break;

                                case 232: // è
                                case 233: // é
                                case 234: // ê
                                case 235: // ë
                                    tmp = tmp.Replace(ch, 'e');
                                    break;

                                case 236: // ì
                                case 237: // í
                                case 238: // î
                                case 239: // ï
                                    tmp = tmp.Replace(ch, 'i');
                                    break;


                                case 240: // ð
                                    tmp = tmp.Replace(ch, 'e');
                                    break;


                                case 241: // ñ
                                    tmp = tmp.Replace(ch, 'n');
                                    break;

                                case 242: // ò
                                case 243: // ó
                                case 244: // ô
                                case 245: // õ
                                case 246: // ö
                                case 248: // ø
                                    tmp = tmp.Replace(ch, 'ö');
                                    break;


                                case 249: // ù
                                case 250: // ú
                                case 251: // û
                                case 252: // ü
                                    tmp = tmp.Replace(ch, 'ü');
                                    break;

                                default:
                                    tmp = tmp.Replace(ch, '_');
                                    break;

                            }

                            foundInvalidChar = true;
                        }
                    }

                    if (foundInvalidChar) {
                        System.Diagnostics.Debug.WriteLine(line + "\t|->\t" + tmp);
                        foundInvalidChar = false;
                    }

                    writer.WriteLine(tmp);
                }
            } catch (Exception ex) {
                MessageBox.Show("Error during execution: " + ex.Message);
                errOccured = true;
            } finally {
                reader.Close();
                writer.Close();
            }

            if (errOccured) {
                File.Delete(txtPath.Text);
                File.Move(txtPath.Text + ".bak", txtPath.Text);
            } else {
                File.Delete(txtPath.Text + ".bak");
                MessageBox.Show("All invalid characters in the selected xml file has been replaced.");
            }
        }
    }
}
