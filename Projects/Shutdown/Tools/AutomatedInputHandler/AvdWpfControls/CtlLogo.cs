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

namespace AvdWpfControls {
    /// <summary>
    /// Führen Sie die Schritte 1a oder 1b und anschließend Schritt 2 aus, um dieses benutzerdefinierte Steuerelement in einer XAML-Datei zu verwenden.
    ///
    /// Schritt 1a) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die im aktuellen Projekt vorhanden ist.
    /// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    /// an der Stelle hinzu, an der es verwendet werden soll:
    ///
    ///     xmlns:MyNamespace="clr-namespace:AvdWpfControls"
    ///
    ///
    /// Schritt 1b) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die in einem anderen Projekt vorhanden ist.
    /// Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei 
    /// an der Stelle hinzu, an der es verwendet werden soll:
    ///
    ///     xmlns:MyNamespace="clr-namespace:AvdWpfControls;assembly=AvdWpfControls"
    ///
    /// Darüber hinaus müssen Sie von dem Projekt, das die XAML-Datei enthält, einen Projektverweis
    /// zu diesem Projekt hinzufügen und das Projekt neu erstellen, um Kompilierungsfehler zu vermeiden:
    ///
    ///     Klicken Sie im Projektmappen-Explorer mit der rechten Maustaste auf das Zielprojekt und anschließend auf
    ///     "Verweis hinzufügen"->"Projekte"->[Navigieren Sie zu diesem Projekt, und wählen Sie es aus.]
    ///
    ///
    /// Schritt 2)
    /// Fahren Sie fort, und verwenden Sie das Steuerelement in der XAML-Datei.
    ///
    ///     <MyNamespace:Logo/>
    ///
    /// </summary>
    public class CtlLogo : Control {

        public CtlLogo() {
            this.DataContext = this;

            if (System.Reflection.Assembly.GetEntryAssembly() != null) {
                if (string.IsNullOrEmpty(this.Caption)) {
                    this.Caption = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                }

                if (string.IsNullOrEmpty(this.Version)) {
                    this.Version = "Version " +
                        System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                        System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Minor + "." +
                        System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build;
                }
            }
        }

        public string Caption {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public string Version {
            get { return (string)GetValue(VersionProperty); }
            set { SetValue(VersionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(CtlLogo), new UIPropertyMetadata(""));

        // Using a DependencyProperty as the backing store for Version.  This enables animation, styling, binding, etc...
        public static DependencyProperty VersionProperty =
            DependencyProperty.Register("Version", typeof(string), typeof(CtlLogo), new UIPropertyMetadata(""));
        
        static CtlLogo() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CtlLogo), new FrameworkPropertyMetadata(typeof(CtlLogo)));
            
        }
    }
}
