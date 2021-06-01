// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-06-12
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKit.Models.GlobalSearch;
using eBalanceKitBase.Windows;
using eBalanceKitBusiness.Structures.GlobalSearch;

namespace eBalanceKit.Windows.GlobalSearch {
    public partial class CtlGlobalSearchContent {
        public CtlGlobalSearchContent() { InitializeComponent(); }

        private GlobalSearchModel Model { get { return DataContext as GlobalSearchModel; } }

        private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (Model == null)
                return;
            Model.SelectedResultItem = e.NewValue as IGlobalSearcherTreeNode;
            SearchResultItem oldValue = (e.OldValue as SearchResultItem);
            if (oldValue != null) {
                List<ISearchableNode> presentationTreePath = oldValue.PresentationTreeEntryPath;
                presentationTreePath[presentationTreePath.Count - 1].SearchLeaveFocus(presentationTreePath);
            }
        }

        private void BtnExpandAllNodesClick(object sender, RoutedEventArgs e) {

            if (Model != null) {
                UIHelpers.TryFindParent<CtlGlobalSearch>(this).slideOutDialog.DisableAnimationTrigger();
                new DlgProgress(UIHelpers.TryFindParent<Window>(this)) {
                    ProgressInfo = {
                        IsIndeterminate = true,
                        Caption = eBalanceKitResources.Localisation.ResourcesCommon.LoadingPositions
                    }
                }.ExecuteModal(ExpandAllNodes, Model);
            }
        }

        private void ExpandAllNodes(object modelObj) {
            var model = (GlobalSearchModel) modelObj;
            Dispatcher.Invoke((Action)(model.ExpandAllNodes), DispatcherPriority.SystemIdle);
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(
                                                                 () => UIHelpers.TryFindParent<CtlGlobalSearch>(this)
                                                                           .slideOutDialog.EnableAnimationTrigger()));
        }


        private void BtnCollapseAllNodesClick(object sender, RoutedEventArgs e) { if (Model != null) Model.CollapseAllNodes(); }

        private void SearchComboBoxKeyUp(object sender, System.Windows.Input.KeyEventArgs e) { if (Model != null) Model.KeyUp(e.Key); }

        private void SearchComboBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            if (Model != null && SearchComboBox.SelectedIndex >= 0) {
                string searchedString = e.AddedItems[0].ToString();
                Model.SelectionChanged(searchedString);
            }
        }

    }
}
