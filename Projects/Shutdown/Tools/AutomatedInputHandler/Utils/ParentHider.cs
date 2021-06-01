using System.Windows;

namespace Utils
{
    public class ParentHider
    {
        #region MainWindow
        private static Window _mainwindow;

        public static Window MainWindow {
            get { return _mainwindow; }
            set {
                if (_mainwindow != value) {
                    _mainwindow = value;
                }
            }
        }
        #endregion MainWindow

        public static Window HideParent(DependencyObject child)
        {
            Window parent = UIHelpers.TryFindParent<Window>(child);
            if (parent != null && parent != MainWindow) {
                parent.Hide();
                return parent;
            }
            return null;
        }

        public static Window HideParent(Window parent)
        {
            if (parent != null && parent != MainWindow) {
                parent.Hide();
                return parent;
            }
            return null;
        }

        public static void ShowDialogParent(Window parent)
        {
            if (parent != null && parent.Visibility == Visibility.Hidden) {
                parent.ShowDialog();
            }
        }

        /// <summary>
        /// Do not use, if need or change DialogResult!!!
        /// </summary>
        /// <param name="parent"></param>
        public static void ShowParent(Window parent)
        {
            if (parent != null && parent.Visibility == Visibility.Hidden) {
                parent.Show();
            }
        }
    }
}
