// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-02-26
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Windows;
using eBalanceKitResources.Localisation;
using eBalanceKitBusiness.Export.Models;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKit.Windows.Export {
    /// <summary>
    /// Interaktionslogik für DlgExportHypercube.xaml
    /// </summary>
    public partial class DlgExportHypercube : Window {
        public DlgExportHypercube(IHyperCube cube) {
            Title = ResourcesExport.ExportOf + " " + cube.Root.Element.Label;
            InitializeComponent();
            DataContext = new ExportHypercubeModel(cube);
        }

        private ExportHypercubeModel Model { get { return DataContext as ExportHypercubeModel; } }
        private void btnCancel_Click(object sender, RoutedEventArgs e) { Close(); }
        private void btnOk_Click(object sender, RoutedEventArgs e) {
            bool? result = Model.Export();
            if (result == null) {
                MessageBox.Show(ResourcesExport.ExportError + ": " + Model.LastExceptionMessage, ResourcesExport.ExportError,
                                MessageBoxButton.OK, MessageBoxImage.Error);
            } else {
                MessageBox.Show(ResourcesExport.ExportSuccessMessage, ResourcesExport.ExportSuccess,
                                MessageBoxButton.OK, MessageBoxImage.Information);
                Close();
            }            
        }
    }
}