using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Taxonomy;
using Utils;
using eBalanceKit.Structures;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Rights;
using eBalanceKitBusiness.Structures;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.XbrlElementValue;

namespace eBalanceKit.Models {
    public class CompanyDisplayValueTreeModel : DisplayValueTreeModel{
        public CompanyDisplayValueTreeModel (ITaxonomy taxonomy, ValueTreeWrapper valueTreeWrapper, ObjectWrapper<eBalanceKitBusiness.Structures.DbMapping.Document> documentWrapper) : base(taxonomy, valueTreeWrapper) {
            ValueTreeWrapper.PropertyChanged += ValueTreeWrapper_PropertyChanged;
            InitSelectedLocationCountry();
            if (documentWrapper != null) {
                documentWrapper.PropertyChanged += documentWrapper_PropertyChanged;
                if (documentWrapper.Value != null) {
                    documentWrapper.Value.PropertyChanged += Value_PropertyChanged;
                    Company = documentWrapper.Value.Company;
                }
            }
        }


        #region Properties
        public Company SelectedParentCompany { get; set; }
        public bool IsAllowedImport { get { return Company != null && RightManager.CompanyWriteAllowed(Company); } }

        private Company _company;
        public Company Company {
            get { return _company; }
            set {
                _company = value;
                OnPropertyChanged("Company");
                OnPropertyChanged("SelectedLocationCountry");
                OnPropertyChanged("IsAllowedImport");
            }
        }

        #region SelectedLocationCountry
        public Country SelectedLocationCountry {
            get { return Company != null ? Company.SelectedLocationCountry : null; }
            set {
                if(Company != null) {
                    Company.SelectedLocationCountry = value;
                    OnPropertyChanged("SelectedLocationCountry");
                }
            }
        }


        private Country _selectedLocationCountry = Country.Countries[0];

        #endregion SelectedLocationCountry

        #endregion Properties

        #region EventHandler
        void documentWrapper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            var docWrapper = sender as ObjectWrapper<eBalanceKitBusiness.Structures.DbMapping.Document>;
            if (docWrapper.Value != null) {
                docWrapper.Value.PropertyChanged -= Value_PropertyChanged;
                docWrapper.Value.PropertyChanged += Value_PropertyChanged;
                Company = docWrapper.Value.Company;
            }
        }

        void Value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "Company") {
                Company = sender is eBalanceKitBusiness.Structures.DbMapping.Document
                              ? ((eBalanceKitBusiness.Structures.DbMapping.Document) sender).Company
                              : ((ObjectWrapper<eBalanceKitBusiness.Structures.DbMapping.Document>) sender).Value.
                                    Company;
            }
        }

        void ValueTreeWrapper_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "ValueTreeRoot") {
                InitSelectedLocationCountry(); 
            }
        }

        private void InitSelectedLocationCountry() {
            if (ValueTreeWrapper.ValueTreeRoot == null) return;
            if (ValueTreeWrapper.ValueTreeRoot.Values.ContainsKey("de-gcd_genInfo.company.id.location.country.isoCode")) {
                var locationIsoValue =
                    ValueTreeWrapper.ValueTreeRoot.Values["de-gcd_genInfo.company.id.location.country.isoCode"];
                if (locationIsoValue.HasValue) {
                    string iso = locationIsoValue.Value.ToString();
                    foreach (Country country in Country.Countries) {
                        if (country.Iso == iso) {
                            this.SelectedLocationCountry = country;
                            break;
                        }
                    }
                }
                else {
                    foreach (Country country in Country.Countries) {
                        if (country.Iso == "DE") {
                            this.SelectedLocationCountry = country;
                            break;
                        }
                    }
                }
            }
        }

        #endregion EventHandler

        public void ApplyParentCompanyValues() {
            if(SelectedParentCompany == null) {
                MessageBox.Show("Keine Firma ausgewählt, um die Daten zu übernehmen.", "", MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }
            string root = "de-gcd_genInfo.company.id";
            string rootParent = "de-gcd_genInfo.company.id.parent";
            var parentElement = (ValueTreeRoot.Values[rootParent] as XbrlElementValue_Tuple).Items[0];
            var otherElement = SelectedParentCompany.ValueTree.Root;

            List<string> directlyMappable = new List<string>() { "name", "name.formerName", "name.dateOfLastChange", 
                "legalStatus.dateOfLastChange",
                "location", "location.street", "location.houseNo", "location.zipCode", "location.city"
            };

            foreach(var element in directlyMappable) {
                parentElement.Values[rootParent + "." + element].Value = otherElement.Values[root + "." + element].Value;
            }

            parentElement.Values[rootParent + ".location.country"].Value = SelectedParentCompany.SelectedLocationCountry.Name;
            parentElement.Values[rootParent + ".location.country.isoCode"].Value = SelectedParentCompany.SelectedLocationCountry.Iso;
            parentElement.Values[rootParent + ".legalStatus.formerStatus"].Value = otherElement.Values[root + ".legalStatus.formerStatus"].Value == null ? "" :
                otherElement.Values[root + ".legalStatus.formerStatus"].DisplayString;
            parentElement.Values[rootParent + ".legalStatus"].Value = otherElement.Values[root + ".legalStatus"].Value == null ? "" :
                otherElement.Values[root + ".legalStatus"].DisplayString;

            List<string> idNos = new List<string>() { "BF4", "ST13", "STID", "HRN", "UID", "STWID", "BKN", "BUN", "IN", "EN", "SN", "S" };
            var idNoRootParent = (parentElement.Values["de-gcd_genInfo.company.id.parent.idNo"] as XbrlElementValue_Tuple).Items[0];
            var idNoRootOther = (otherElement.Values["de-gcd_genInfo.company.id.idNo"] as XbrlElementValue_Tuple).Items[0];
            root = "de-gcd_genInfo.company.id.idNo.type.companyId.";
            foreach(var idNo in idNos) {
                idNoRootParent.Values[root + idNo].Value = idNoRootOther.Values[root + idNo].Value;

            }
        }
    }
}
