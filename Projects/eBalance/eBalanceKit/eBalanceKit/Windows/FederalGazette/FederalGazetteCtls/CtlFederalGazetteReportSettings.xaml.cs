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
using System.Windows.Navigation;
using System.Windows.Shapes;
using eBalanceKitBusiness.FederalGazette;
using eBalanceKitBusiness.FederalGazette.Model;

namespace eBalanceKit.Windows.FederalGazette.FederalGazetteCtls
{
    /// <summary>
    /// Interaktionslogik für CtlFederalGazetteReportSettings.xaml
    /// </summary>
    public partial class CtlFederalGazetteReportSettings : UserControl
    {
        public CtlFederalGazetteReportSettings(FederalGazetteMainModel model)
        {
            InitializeComponent();
            Model = model;
            DataContext = Model;
        }

        private FederalGazetteMainModel Model { get; set; }


    }
}
