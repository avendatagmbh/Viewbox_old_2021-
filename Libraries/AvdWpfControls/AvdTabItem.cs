using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AvdWpfControls
{
    ///<summary>
    ///  Führen Sie die Schritte 1a oder 1b und anschließend Schritt 2 aus, um dieses benutzerdefinierte Steuerelement in einer XAML-Datei zu verwenden. Schritt 1a) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die im aktuellen Projekt vorhanden ist. Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei an der Stelle hinzu, an der es verwendet werden soll: xmlns:MyNamespace="clr-namespace:AvdWpfControls" Schritt 1b) Verwenden des benutzerdefinierten Steuerelements in einer XAML-Datei, die in einem anderen Projekt vorhanden ist. Fügen Sie dieses XmlNamespace-Attribut dem Stammelement der Markupdatei an der Stelle hinzu, an der es verwendet werden soll: xmlns:MyNamespace="clr-namespace:AvdWpfControls;assembly=AvdWpfControls" Darüber hinaus müssen Sie von dem Projekt, das die XAML-Datei enthält, einen Projektverweis zu diesem Projekt hinzufügen und das Projekt neu erstellen, um Kompilierungsfehler zu vermeiden: Klicken Sie im Projektmappen-Explorer mit der rechten Maustaste auf das Zielprojekt und anschließend auf "Verweis hinzufügen"->"Projekte"->[Navigieren Sie zu diesem Projekt, und wählen Sie es aus.] Schritt 2) Fahren Sie fort, und verwenden Sie das Steuerelement in der XAML-Datei. <MyNamespace:AvdTabItem />
    ///</summary>
    public class AvdTabItem : TabItem
    {
        static AvdTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (AvdTabItem),
                                                     new FrameworkPropertyMetadata(typeof (AvdTabItem)));
        }

        #region Caption

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof (string), typeof (AvdTabItem),
                                        new UIPropertyMetadata(string.Empty));

        public string Caption
        {
            get { return (string) GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        #endregion Caption

        #region ImageSource

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof (ImageSource), typeof (AvdTabItem),
                                        new UIPropertyMetadata(null));

        public ImageSource ImageSource
        {
            get { return (ImageSource) GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        #endregion ImageSource

        #region HorizontalAlignment

        // Using a DependencyProperty as the backing store for HorizontalAlignment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderHorizontalAlignmentProperty =
            DependencyProperty.Register("HeaderHorizontalAlignment", typeof (HorizontalAlignment), typeof (AvdTabItem),
                                        new UIPropertyMetadata(HorizontalAlignment.Center));

        public HorizontalAlignment HeaderHorizontalAlignment
        {
            get { return (HorizontalAlignment) GetValue(HeaderHorizontalAlignmentProperty); }
            set { SetValue(HeaderHorizontalAlignmentProperty, value); }
        }

        #endregion HorizontalAlignment
    }
}