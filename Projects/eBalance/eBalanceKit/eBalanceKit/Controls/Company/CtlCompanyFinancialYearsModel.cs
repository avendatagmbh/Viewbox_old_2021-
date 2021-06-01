using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.Structures;
using Taxonomy.PresentationTree;
using Taxonomy;
using eBalanceKitBusiness.Manager;

namespace eBalanceKit.Controls.Company {

    /// <summary>
    /// Model for the CtlCompanyFinancialYears control.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-18</since>
    internal class CtlCompanyFinancialYearsModel : INotifyPropertyChanged {

        #region constructor
        public CtlCompanyFinancialYearsModel() {

            Taxonomy = TaxonomyManager.GCD_Taxonomy;
            PresentationTree = Taxonomy.PresentationTrees.First();

            Elements = Taxonomy.Elements;
           
            FinancialYearValues = new List<int>();
            for (int i = 2010; i <= 2030; i++) {
                FinancialYearValues.Add(i);
            }
        }
        #endregion constructor

        #region events
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        #region properties

        #region Company
        public eBalanceKitBusiness.Structures.DbMapping.Company Company {
            get {
                return _company;
            }
            set {
                if (_company == value)
                    return;
                _company = value;
                OnPropertyChanged("Company");
                OnPropertyChanged("IsEditAllowed");

                if (_company != null) {
                    for (int i = 0; i < this.Company.FinancialYears.Count; i++) {
                        if (this.Company.FinancialYears[i].IsEnabled) {
                            for (int j = this.Company.FinancialYears.Count - 1; j > i; j--) {
                                if (this.Company.FinancialYears[j].IsEnabled) {
                                    //Prevent OnPropertyChanged, do this later when it is save
                                    _selectedFinancialYearFrom = this.Company.FinancialYears[i].FYear;
                                    _selectedFinancialYearTo = this.Company.FinancialYears[j].FYear;
                                    break;
                                }
                            }
                            break;
                        }
                    }

                    if (SelectedFinancialYearFrom == 0) _selectedFinancialYearFrom = System.DateTime.Now.Year;
                    if (SelectedFinancialYearTo == 0) _selectedFinancialYearTo = System.DateTime.Now.Year;
                    OnPropertyChanged("SelectedFinancialYearFrom");
                    OnPropertyChanged("SelectedFinancialYearTo");

                    this.Company.SetVisibleFinancialYearIntervall(this.SelectedFinancialYearFrom, this.SelectedFinancialYearTo);
                }
            }
        }
        private eBalanceKitBusiness.Structures.DbMapping.Company _company;

        #endregion
        public Taxonomy.ITaxonomy Taxonomy { get; set; }
        public Dictionary<string, Taxonomy.IElement> Elements { get; set; }
        private IPresentationTree PresentationTree { get; set; }
        public List<int> FinancialYearValues { get; set; }

        #region SelectedFinancialYearFrom
        public int SelectedFinancialYearFrom {
            get { return _selectedFinancialYearFrom; }
            set {
                //if (DocumentManager.Instance.Documents.Any(doc => doc.FinancialYear.FYear < value)) {
                //    return;
                //}
                _selectedFinancialYearFrom = value;
                OnPropertyChanged("SelectedFinancialYearFrom");
            }
        }
        int _selectedFinancialYearFrom;
        #endregion

        #region SelectedFinancialYearTo
        public int SelectedFinancialYearTo {
            get { return _selectedFinancialYearTo; }
            set {
                //if (DocumentManager.Instance.Documents.Any(doc => doc.FinancialYear.FYear > value)) {
                //    return;
                //}
                _selectedFinancialYearTo = value;
                OnPropertyChanged("SelectedFinancialYearTo");
            }
        }
        int _selectedFinancialYearTo;
        #endregion

        public bool IsEditAllowed { get { return RightManager.CompanyWriteAllowed(Company); } }

        public object SelectedItem { get; set; }
        #endregion properties
        
    }
}