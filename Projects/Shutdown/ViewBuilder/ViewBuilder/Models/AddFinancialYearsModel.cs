using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SystemDb;
using SystemDb.Compatibility.Viewbuilder.OptimizationRelated;
using Utils;

namespace ViewBuilder.Models {
    public class AddFinancialYearsModel : NotifyPropertyChangedBase {
        #region Constructor
        public AddFinancialYearsModel(List<Optimization> optimizations, OptimizationGroup group, ILanguageCollection languages, Layers layers) {
            _layers = layers;
            _languages = languages;
            _group = group;
            Optimizations = optimizations;
            IsCheckedHeaderState = true;
            StartYear = 2000;
            EndYear = 2010;
            foreach(var opt in Optimizations)
                opt.PropertyChanged += opt_PropertyChanged;
        }

        void opt_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == "IsChecked" && _updateByOpts) {
                bool value = (sender as Optimization).IsChecked;
                foreach (var opt in Optimizations)
                    if (value != opt.IsChecked) {
                        IsCheckedHeaderState = null;
                        return;
                    }
                IsCheckedHeaderState = value;
            }
        }
        #endregion Constructor

        #region Properties
        public List<Optimization> Optimizations { get; private set; }

        private bool _updateByOpts = true;

        #region AddFinancialYearAll
        private bool _addFinancialYearAll;

        public bool AddFinancialYearAll {
            get { return _addFinancialYearAll; }
            set {
                if (_addFinancialYearAll != value) {
                    _addFinancialYearAll = value;
                    OnPropertyChanged("AddFinancialYearAll");
                }
            }
        }
        #endregion AddFinancialYearAll

        #region IsCheckedHeaderState
        private bool? _isCheckedHeaderState;

        public bool? IsCheckedHeaderState {
            get { return _isCheckedHeaderState; }
            set {
                if (_isCheckedHeaderState != value) {
                    _isCheckedHeaderState = value;
                    if (value.HasValue) {
                        _updateByOpts = false;
                        foreach (var opt in Optimizations)
                            opt.IsChecked = value.Value;
                        _updateByOpts = true;
                    }
                    OnPropertyChanged("IsCheckedHeaderState");
                }
            }
        }
        #endregion IsCheckedHeaderState

        #region StartYear
        private int _startYear;

        public int StartYear {
            get { return _startYear; }
            set {
                if (_startYear != value) {
                    _startYear = value;
                    OnPropertyChanged("StartYear");
                }
            }
        }
        #endregion StartYear

        #region EndYear
        private int _endYear;
        private OptimizationGroup _group;
        private ILanguageCollection _languages;
        private Layers _layers;

        public int EndYear {
            get { return _endYear; }
            set {
                if (_endYear != value) {
                    _endYear = value;
                    OnPropertyChanged("EndYear");
                }
            }
        }
        #endregion EndYear

        //public bool Saved { get; private set; }
        #endregion Properties

        #region Methods
        public void Save() {
            //Saved = true;
            foreach (var opt in Optimizations) {
                if (opt.IsChecked) {
                    opt.Children.Clear();
                    for(int i = StartYear; i <= EndYear; ++i )
                        opt.Children.Add(new Optimization(opt.Descriptions.Count, _layers, opt) { Value = i.ToString(), Group = _group });
                    if (AddFinancialYearAll) {
                        Optimization allYearOpt = new Optimization(opt.Descriptions.Count, _layers, opt) { Value = null, Group = _group };
                        int index = 0;
                        foreach (var lang in _languages) {
                            if (lang.CountryCode == "de")
                                allYearOpt.Descriptions[index] = "Alle";
                            if (lang.CountryCode == "en")
                                allYearOpt.Descriptions[index] = "All";
                            index++;
                        }
                        opt.Children.Add(allYearOpt);
                    }
                }
            }
        }
        #endregion Methods

    }
}
