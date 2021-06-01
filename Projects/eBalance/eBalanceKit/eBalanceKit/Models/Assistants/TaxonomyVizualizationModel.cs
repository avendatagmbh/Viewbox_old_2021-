// --------------------------------------------------------------------------------
// author: Sebastian Vetter
// since: 2012-10-18
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using Taxonomy.Enums;
using Taxonomy.Interfaces;

namespace eBalanceKit.Models.Assistants {
    public class TaxonomyVizualizationModel : Utils.NotifyPropertyChangedBase {
        public TaxonomyVizualizationModel() {

            _taxonomyTypes = System.Enum.GetValues(typeof (Taxonomy.Enums.TaxonomyType));
            AvailableTaxonomies = new Dictionary<TaxonomyType, ITaxonomyInfo>();
            foreach (TaxonomyType taxonomyType in _taxonomyTypes) {
                AvailableTaxonomies.Add(taxonomyType, null);
            }

            AvailableVersions = eBalanceKitBusiness.Manager.TaxonomyManager.GetAvailableTaxonomyVersions();
            SelectedVersion = AvailableVersions.First();
        }

        private System.Array _taxonomyTypes;

        #region AvailableVersions
        private IEnumerable<string> _availableVersions;

        public IEnumerable<string> AvailableVersions {
            get { return _availableVersions; }
            set {
                if (_availableVersions != value) {
                    _availableVersions = value;
                    OnPropertyChanged("AvailableVersions");
                }
            }
        }
        #endregion AvailableVersions

        #region SelectedVersion
        private string _selectedVersion;

        public string SelectedVersion {
            get { return _selectedVersion; }
            set {
                if (_selectedVersion != value) {
                    _selectedVersion = value;
                    SetAvailableTaxonomies();

                    OnPropertyChanged("SelectedVersion");
                }
            }
        }
        #endregion SelectedVersion

        private void SetAvailableTaxonomies() {

            foreach (TaxonomyType type in _taxonomyTypes) {
                AvailableTaxonomies[type] = null;
            }

            var availableTaxonomies =
                eBalanceKitBusiness.Manager.TaxonomyManager.GetAvailableTaxonomies(_selectedVersion);
            //AvailableTaxonomies = availableTaxonomies.ToDictionary(availableTaxonomy => availableTaxonomy.Type);

            foreach (var avTaxonomy in availableTaxonomies) {
                AvailableTaxonomies[avTaxonomy.Type] = avTaxonomy;
            }
                    
            OnPropertyChanged("AvailableTaxonomies");
        }

        #region AvailableTaxonomies
        private Dictionary<TaxonomyType, ITaxonomyInfo> _availableTaxonomies;

        public Dictionary<TaxonomyType, ITaxonomyInfo> AvailableTaxonomies {
            get { return _availableTaxonomies; }
            set {
                if (_availableTaxonomies != value) {
                    _availableTaxonomies = value;
                    OnPropertyChanged("AvailableTaxonomies");
                }
            }
        }
        #endregion AvailableTaxonomies

        //public void GenerateVizualization(TaxonomyType taxonomyType, string filename) {
        //    var content = eBalanceKitBusiness.Manager.TaxonomyManager.GetTaxonomyVizualization(taxonomyType, SelectedVersion);
        //    System.IO.File.WriteAllText(filename, content, System.Text.Encoding.UTF8);
        //}

    }


}