using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Controls.FederalGazette {
    /// <summary>
    /// Interaktionslogik für CtlIntroduction.xaml
    /// </summary>
    public partial class CtlIntroduction : UserControl {
        public CtlIntroduction() {
            InitializeComponent();
        }

        private void OpenRegistrationPDFClick(object sender, RoutedEventArgs e) {
            
            OpenFile("D001_registration_info.pdf", "Resources");

            //var stream = Assembly
            //    .GetExecutingAssembly()
            //    .GetManifestResourceStream("federalGazetteBusiness.Resources.D001_registration_info.pdf");
            //FileStream f = new FileStream("registrationInfo.pdf", FileMode.OpenOrCreate);
            ////Properties.Resources.


            ////Write Bytes into Our Created help.pdf
            //MemoryStream ms = new MemoryStream(stream);
            //stream.WriteTo(f);

            //f.Close();

            //stream.Close();


            //// Finally Show the Created PDF from resources

            //Process.Start("registrationInfo.pdf");
        }

        private void OpenFile(string fileName, string fileFolder) {

            var loc = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = System.IO.Path.GetDirectoryName(loc);
            if (string.IsNullOrEmpty(directory)) {
                var win = Utils.UIHelpers.TryFindParent<Window>(this);
                MessageBox.Show(win,
                                "Die Datei konnte nicht gefunden werden. Bitte prüfen Sie, ob die Datei " + fileName +
                                " im Unterordner '" + fileFolder + "' des Programmverzeichnisses vorhanden ist.",
                                "Datei nicht gefunden");
                return;
            }
            System.Diagnostics.Process.Start(
                System.IO.Path.Combine(directory, fileFolder, fileName));
            
        }

        private void OpenWebservicePDFClick(object sender, RoutedEventArgs e) {
            //throw new NotImplementedException("PDF noch nicht verfügbar.");
            OpenFile("Faxformular Einsender-AvenDATA.pdf", "Resources");
        }
    }
}
