using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Taxonomy;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using Taxonomy.Enums;
using eBalanceKit.Windows;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit {

    public class GlobalInfo : INotifyPropertyChanged {

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion
        
        #region SelectedElement
        private IElement _selectedElement;
        public IElement SelectedElement {
            get { return _selectedElement; }
            set {
                if (_selectedElement != value) {
                    _selectedElement = value;
                    OnPropertyChanged("SelectedElement");
                    OnPropertyChanged("InfoHeaderText");
                }
            }
        }
        #endregion

        #region BalanceListSelectedIndex
        private int _balanceListSelectedIndex;
        public int BalanceListSelectedIndex {
            get { return _balanceListSelectedIndex; }
            set {
                if (_balanceListSelectedIndex != value) {
                    _balanceListSelectedIndex = value;
                    OnPropertyChanged("BalanceListSelectedIndex");
                }
            }
        }
        #endregion

        public string InfoHeaderText { get { return SelectedElement == null ? "Kein Element ausgewählt" : SelectedElement.ShortDocumentation; } }
    }

    public sealed class GlobalResources {

        static GlobalResources() {
            ImageSourceLogo =
                new BitmapImage(new Uri(@"pack://application:,,,/CustomResources;component/Resources/logo1.png"));
        }

        public static ImageSource ImageSourceLogo { get; private set; }
        
        /// <summary>
        /// Reference to main window.
        /// </summary>
        public static MainWindow MainWindow { get; set; }

        //public static SolidColorBrush ComputationSourceBgBrush = new SolidColorBrush(Color.FromRgb(0xD9, 0xEF, 0x9F));
        //public static SolidColorBrush InfoBgBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xBA));
        public static SolidColorBrush ComputationSourceBgBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF)); /*SolidColorBrush(Color.FromArgb(0xFF, 0xF9, 0xF5, 0xBB));*/
        public static SolidColorBrush InfoBgBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        public static SolidColorBrush ComputedBgBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
        public static SolidColorBrush TextboxBgBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        public static SolidColorBrush SelectedBorderBrush = new SolidColorBrush(Colors.PowderBlue);
        public static Thickness SelectedElementThickness = new Thickness(3d);
        //public static SolidColorBrush DefaultTextBoxBorderBrush = new SolidColorBrush(Color.FromRgb(0xAA, 0xAA, 0xAA));
        //public static Thickness DefaultTextBoxThickness = new Thickness(1d); 
        //public static SolidColorBrush DefaultComboBoxBorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0));
        //public static Thickness DefaultComboBoxThickness = new Thickness(0d);

        //public static LinearGradientBrush DefaultBooleanBorderBrush =
        //    new LinearGradientBrush(
        //        new GradientStopCollection(new[] {
        //            new GradientStop(Color.FromRgb(0xCC, 0xCC, 0xCC), 0.0d),
        //            new GradientStop(Color.FromRgb(0x44, 0x44, 0x44), 1.0d)
        //        }),
        //        new Point(0, 0), new Point(0, 1));
        //public static Thickness DefaultBooleanThickness = new Thickness(1d);
        //public static SolidColorBrush DefaultDateBorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0x20, 0x20, 0x20));
        //public static Thickness DefaultDateThickness = new Thickness(1d);

        public static GlobalInfo Info = new GlobalInfo();
        
        internal static void UpdateTabSelection(object sender) {
            foreach (TabItem tabitem in (sender as TabControl).Items) {

                TextBlock header;
                if (tabitem.Header is TextBlock) {
                    header = tabitem.Header as TextBlock;
                } else {
                    var h = tabitem.Header as DependencyObject;
                    if (h != null) return; // e.g. if header is just a string value
                    header = UIHelpers.FindVisualChild<TextBlock>(tabitem.Header as DependencyObject);
                    if (header == null) return;
                }

                if (tabitem.IsSelected) {
                        (header as TextBlock).Foreground = Brushes.Black;
                    } else {
                        (header as TextBlock).Foreground = Brushes.White;
                    }

                    if (tabitem.IsSelected) {
                        (header as TextBlock).Foreground = Brushes.Black;
                    } else {
                        (header as TextBlock).Foreground = Brushes.White;
                    }
                    if (tabitem.IsEnabled) {
                    } else {
                        (header as TextBlock).Foreground = Brushes.Silver;
                    }

            }
        }
    }
}