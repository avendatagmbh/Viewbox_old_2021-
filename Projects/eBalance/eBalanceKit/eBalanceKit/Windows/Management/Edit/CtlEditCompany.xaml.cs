// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-22
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using eBalanceKit.Controls.Company;
using eBalanceKit.Models;
using eBalanceKit.Structures;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKit.Windows.Management.Edit {
    public sealed partial class CtlEditCompany {
        public CtlEditCompany() {
            InitializeComponent();

            if (IsInDesignMode) return;
         
            DataContextChanged += (sender, args) => {
                ValueTreeWrapper = new ValueTreeWrapper();
                ValueTreeWrapper.ValueTreeRoot = Company.ValueTree.Root;

                var ctl = new CtlCompanyInfo(ValueTreeWrapper) {
                    DataContext = new CompanyDisplayValueTreeModel(
                        TaxonomyManager.GCD_Taxonomy, ValueTreeWrapper, null) { Company = Company }
                };

                AddChild(ctl); 
                
            };
        }

        public Company Company { get { return (Company)DataContext; } }
        private ValueTreeWrapper ValueTreeWrapper { get; set; }

        public bool IsInDesignMode { get { return (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "devenv"); } }
    }
}