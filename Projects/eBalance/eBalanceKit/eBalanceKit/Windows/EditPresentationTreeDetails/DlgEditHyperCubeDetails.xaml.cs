// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-01-02
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Windows;
using eBalanceKit.Windows.Export;
using eBalanceKit.Windows.Import;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.Interfaces.PresentationTree;

namespace eBalanceKit.Windows.EditPresentationTreeDetails {
    public partial class DlgEditHyperCubeDetails {
        public DlgEditHyperCubeDetails(IPresentationTreeNode root) {
            InitializeComponent();

            Cube = root.Document.GetHyperCube(root.Element.Id);

            Title = root.Element.Label;

            switch (Cube.Dimensions.AllDimensionItems.Count()) {
                case 2:
                    ctlHyperCubeTable.Visibility = Visibility.Visible;
                    DataContext = Cube.GetTable(Cube.Dimensions.Primary, Cube.Dimensions.DimensionItems.Last());
                    break;

                case 3:
                    // TODO: remove fixed dimension order!
                    ctlHyperCube3DCube.Visibility = Visibility.Visible;
                    DataContext = Cube.Get3DCube(
                        Cube.Dimensions.Primary,
                        Cube.Dimensions.DimensionItems.Last(),
                        Cube.Dimensions.DimensionItems.First());
                    break;

                default:
                    throw new Exception(Cube.Dimensions.DimensionItems.Count() + "-dimensional hypercubes are not yet supported.");
            }
        }

        public IHyperCube Cube { get; set; }

        private void BtnOkClick(object sender, RoutedEventArgs e) {
            DialogResult = true;          
        }

        //private void BtnImportClick(object sender, RoutedEventArgs e) { new DlgImportHypercube(Cube) {Owner = this}.ShowDialog(); }
        private void BtnImportClick(object sender, RoutedEventArgs e) { new DlgImportHyperCube(Cube) {Owner = this}.ShowDialog(); }

        private void BtnExportClick(object sender, RoutedEventArgs e) { new DlgExportHypercube(Cube) { Owner = this }.ShowDialog(); }

        private void BtnTemplateClick(object sender, RoutedEventArgs e) { new DlgTemplateConfig(Cube) {Owner = this}.ShowDialog(); }

    }
}