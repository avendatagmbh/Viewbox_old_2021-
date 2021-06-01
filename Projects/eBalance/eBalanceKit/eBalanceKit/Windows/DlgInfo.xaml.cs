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
using System.Windows.Shapes;
using System.Diagnostics;
using eBalanceKitBusiness.Structures;

namespace eBalanceKit.Windows {
    /// <summary>
    /// Interaktionslogik für DlgInfo.xaml
    /// </summary>
    public partial class DlgInfo : Window {
        public DlgInfo() {
            InitializeComponent();
            supportMail.Inlines.Clear();
            supportMail.Inlines.Add(new Run(CustomResources.CustomStrings.SupportMail));
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
                (System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build > 0 ? "." + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build : "");

            supportMail.NavigateUri = new Uri(AppConfig.GetSupportMailHRef());
        }

    }
}
