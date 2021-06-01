using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Algorithm.Interfaces;
using eBalanceKitBusiness.MappingTemplate;
using eBalanceKitBusiness.Structures;
using System.Collections.ObjectModel;
using Utils;
using System.Collections;

namespace eBalanceKitBusiness.Algorithm {
    public class AssignmentList : NotifyPropertyChangedBase, IFilterList
    {

        public AssignmentList(IEnumerable<MappingHeaderGui> assignments) {
            _assignments = assignments;

            DisplayedAssignments = new ObservableCollection<MappingHeaderGui>();
            foreach (var mappingHeaderGui in assignments) {
                DisplayedAssignments.Add(mappingHeaderGui);
            }
        }

        private readonly IEnumerable<MappingHeaderGui> _assignments;
        public BalanceListFilterOptions ItemsFilter { get; set; }
        public BalanceListSortOptions SortOptions { get; set; }
        public ObservableCollection<MappingHeaderGui> DisplayedAssignments { get; set; }

        public int GetAssignmentsCount() {
            if (_assignments == null)
                return 0;

            return _assignments.Count(); 
        }

        public bool HasUnassignedItems { get { return _assignments.Any(); } }
        public bool HasDisplayedUnassignedItems { get { return DisplayedAssignments.Any(); } }

        public void UpdateFilter()
        {
            IEnumerable<MappingHeaderGui> tempList = null;
            switch(SortOptions.SortType)
            {
                case BalanceListSortType.AccountName:
                    tempList = _assignments.OrderBy(a => a.Name).Where(item => FilterManager.GetVisibility(item, ItemsFilter));
                    break;

                case BalanceListSortType.AccountNumber:
                    tempList = _assignments.OrderBy(a => a.Number).Where(item => FilterManager.GetVisibility(item, ItemsFilter));
                    break;

                case BalanceListSortType.Index:
                    tempList = _assignments.OrderBy(a => a.SortIndex).Where(item => FilterManager.GetVisibility(item, ItemsFilter));
                    break;

                case BalanceListSortType.Original:
                    tempList = _assignments.Where(item => FilterManager.GetVisibility(item, ItemsFilter));
                    break;
            }

            DisplayedAssignments.Clear();
            foreach (MappingHeaderGui item in tempList)
                DisplayedAssignments.Add(item);
            
            OnPropertyChanged("DisplayedAssignments");
            OnPropertyChanged("HasUnassignedItems");
            OnPropertyChanged("HasDisplayedUnassignedItems");
        }
    }
}
