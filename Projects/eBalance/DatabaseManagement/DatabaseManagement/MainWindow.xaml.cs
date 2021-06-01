// --------------------------------------------------------------------------------
// author: Benjamin Held
// since:  2011-07-26
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using DatabaseManagement.Manager;
using DatabaseManagement.Models;

namespace DatabaseManagement {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
            //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture("de-DE");

            try {
                Model = new MainWindowModel(this);
                DataContext = Model;
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                Close();
                return;
            }
        }

        private MainWindowModel Model { get; set; }

        private void WindowClosed(object sender, EventArgs e) { DatabaseManager.ConnectionManager.Dispose(); }

    }
}