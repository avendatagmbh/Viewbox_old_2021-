// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace TransDATA.Windows {
    /// <summary>
    /// Interaktionslogik für DlgInfo.xaml
    /// </summary>
    public partial class DlgInfo : Window {
        public DlgInfo() {
            InitializeComponent();
        }

        private void WindowPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                Close();
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e) {
            ctnInfo.txtVersion.Text = "Version " +
                                      Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                                      Assembly.GetEntryAssembly().GetName().Version.Minor +
                                      (Assembly.GetEntryAssembly().GetName().Version.Build > 0
                                           ? "." + Assembly.GetEntryAssembly().GetName().Version.Build
                                           : "");
        }
    }
}