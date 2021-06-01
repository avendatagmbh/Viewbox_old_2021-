// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace eBalanceKit.Windows.Management.Management {
    public partial class DlgManagementList {
        public DlgManagementList(string title, string listCaption, string imageName, IEnumerable<object> items) {
            InitializeComponent();
            Title = title;
            txtListCaption.Text = listCaption;
            Icon = new BitmapImage(new Uri("pack://application:,,,/eBalanceKitResources;component/Resources/" + imageName));
            DataContext = items;
        }

        private void BtnOkClick(object sender, System.Windows.RoutedEventArgs e) { Close(); }
        private void WindowKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Escape) DialogResult = false; }
    }
}