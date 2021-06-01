// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-07-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Taxonomy.PresentationTree;

namespace eBalanceKitBusiness.MappingTemplate {
    public class MappingGuiBase : PresentationTreeEntry {
        #region IsExpanded
        private bool _isExpanded;

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded != value) {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }
        #endregion

        #region IsSelected
        private bool _isSelected;

        public virtual bool IsSelected {
            get { return _isSelected; }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");

                    //if (_isSelected) IsSelectedAssignments = true;
                }
            }
        }
        #endregion

        #region IsExpandedAssignments
        private bool _isExpandedAssignments;

        public bool IsExpandedAssignments {
            get { return _isExpandedAssignments; }
            set {
                if (_isExpandedAssignments != value) {
                    _isExpandedAssignments = value;
                    OnPropertyChanged("IsExpandedAssignments");
                }
            }
        }
        #endregion

        #region IsSelectedAssignments
        private bool _isSelectedAssignments;

        public bool IsSelectedAssignments {
            get { return _isSelectedAssignments; }
            set {
                if (_isSelectedAssignments != value) {
                    _isSelectedAssignments = value;
                    OnPropertyChanged("IsSelectedAssignments");

                    if (_isSelectedAssignments) IsSelected = true;
                }
            }
        }
        #endregion

        public virtual string AccountNumber { get; set; }
        public virtual string AccountName { get; set; }

        public bool CheckFilter(string filter, bool exactSearch, bool searchAccountNumbers, bool searchAccountNames) {
            if (string.IsNullOrEmpty(filter)) return true;

            bool result = false;

            if (exactSearch) {
                if (searchAccountNumbers) {
                    result |= AccountNumber.StartsWith(filter);
                }

                if (searchAccountNames) {
                    result |= AccountName.StartsWith(filter);
                }
            } else {
                try {
                    //Regex regex = new Regex(filter);
                    if (searchAccountNumbers) {
                        result |= AccountNumber.Contains(filter);
                        //result |= regex.IsMatch(this.Number);
                    }

                    if (searchAccountNames) {
                        result |= AccountName.Contains(filter);
                        //result |= regex.IsMatch(this.Name);
                    }
                } catch (Exception) {}
            }

            return result;
        }
    }
}