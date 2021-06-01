// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-06
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Base.Localisation;
using Business;
using Business.Interfaces;
using Config.Interfaces.DbStructure;

namespace TransDATA.Controls {
    /// <summary>
    /// Interaktionslogik für CtlExport.xaml
    /// </summary>
    public partial class CtlExport : UserControl {
        public CtlExport() { InitializeComponent(); }

        private IExporter Exporter { get; set; }

        private void BtnCancelClick(object sender, RoutedEventArgs e) {
            if (MessageBox.Show(ResourcesCommon.RequestCancelExport, ResourcesCommon.RequestCancelExportCaption,
                                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
                MessageBoxResult.No)
                return;

            Exporter.Cancel();
            UIHelpers.TryFindParent<Window>(this).Close();
        }

        public void StartExport(IProfile profile) {
            Exporter = AppController.GetExporter(profile);
            Exporter.Finished += ExporterFinished;
            Exporter.Start();
            DataContext = Exporter.TransferProgress;
        }

        private void ExporterFinished(object sender, System.EventArgs e) {
            if (Dispatcher.CheckAccess()) {
                var owner = UIHelpers.TryFindParent<Window>(this);
                owner.Close();
                MessageBox.Show(owner.Owner, ResourcesCommon.ExportFinished, ResourcesCommon.ExportFinishedCaption,
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                
            } else {
                Dispatcher.Invoke(new EventHandler(ExporterFinished), new [] {sender, e});
            }
        }
    }
}