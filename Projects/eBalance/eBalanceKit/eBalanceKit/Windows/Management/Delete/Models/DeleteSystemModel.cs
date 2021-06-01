// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-20
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Utils;
using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKit.Windows.Management.Delete.Models {
    public class DeleteSystemModel : NotifyPropertyChangedBase {
        public DeleteSystemModel(Window owner) {
            Owner = owner;
            _items = new ObservableCollectionAsync<CheckableItemWrapper<eBalanceKitBusiness.Structures.DbMapping.System>>();
            foreach (var item in
                (from s in SystemManager.Instance.Systems
                 where !s.HasAnyAssignedDocument
                 select new CheckableItemWrapper<eBalanceKitBusiness.Structures.DbMapping.System>(s))) _items.Add(item);

            foreach (var item in Items)
                item.PropertyChanged += (sender, args) => OnPropertyChanged("DeleteButtonEnabled");
        }

        private Window Owner { get; set; }

        #region Items
        private readonly ObservableCollectionAsync<CheckableItemWrapper<eBalanceKitBusiness.Structures.DbMapping.System>>_items;
        public IEnumerable<CheckableItemWrapper<eBalanceKitBusiness.Structures.DbMapping.System>> Items { get { return _items; } }
        #endregion // Items

        public void DeleteSelectedItems() {
            if (
                MessageBox.Show(Owner, ResourcesManamgement.RequestDeleteAllSelectedSystems, string.Empty, MessageBoxButton.YesNo,
                                MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes) return;

            try {
                foreach (var system in SelectedItems.ToList()) {
                    SystemManager.Instance.DeleteSystem(system.Item);
                    _items.Remove(system);
                }
            } catch (Exception ex) {
                throw new Exception(ResourcesManamgement.DeleteAllSelectedSystemsFailed + ex.Message);
            }

            MessageBox.Show(Owner, ResourcesManamgement.DeleteAllSelectedSystemsSucceed);
            
        }

        public IEnumerable<CheckableItemWrapper<eBalanceKitBusiness.Structures.DbMapping.System>> SelectedItems { get { return from s in Items where s.IsChecked select s; } }

        #region DeleteButtonEnabled
        public bool DeleteButtonEnabled { get { return SelectedItems.Any(); } }
        #endregion // DeleteButtonEnabled
    }
}