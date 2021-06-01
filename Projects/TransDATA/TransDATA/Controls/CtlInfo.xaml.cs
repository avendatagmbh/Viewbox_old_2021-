// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-08-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using Business;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlInfo.xaml
    /// </summary>
    public partial class CtlInfo : UserControl {
        public CtlInfo() {
            InitializeComponent();

            // workaround due to designer support
            if (Assembly.GetEntryAssembly() != null)
                supportMail.NavigateUri = new Uri(AppController.GetSupportMailHRef());
        }

        private Window Owner {
            get { return UIHelpers.TryFindParent<Window>(this); }
        }

        private void UserControlPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                e.Handled = true;
                Owner.Close();
            }
        }

        private void BtnCloseClick(object sender, RoutedEventArgs e) {
            Owner.Close();
        }

        private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e) {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}