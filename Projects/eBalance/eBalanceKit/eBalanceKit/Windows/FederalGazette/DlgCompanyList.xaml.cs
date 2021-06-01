using eBalanceKitBusiness.FederalGazette;
using eBalanceKitBusiness.FederalGazette.Model;

namespace eBalanceKit.Windows.FederalGazette
{
    /// <summary>
    /// Interaktionslogik für DlgCompanyList.xaml
    /// </summary>
    public partial class DlgCompanyList 
    {
        public DlgCompanyList()
        {
            InitializeComponent();
        }

        public CompanyList Model { get; set; }
    }
}
