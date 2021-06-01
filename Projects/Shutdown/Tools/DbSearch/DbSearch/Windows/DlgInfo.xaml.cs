using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Diagnostics;

namespace DbSearch.Windows {
    /// <summary>
    /// Interaktionslogik für DlgInfo.xaml
    /// </summary>
    public partial class DlgInfo : Window {
        public DlgInfo() {
            InitializeComponent();
            //supportMail.Inlines.Clear();
            //supportMail.Inlines.Add(new Run(CustomResources.CustomStrings.SupportMail));
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) { Close(); }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) Close(); }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            this.txtVersion.Text = "Version " +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Minor +
                "." + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build +
                "." + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.MinorRevision;

           supportMail.NavigateUri = new Uri("mailto:b.held@avendata.de");
        }

    }
}
