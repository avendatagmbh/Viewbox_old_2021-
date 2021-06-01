// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-11-04
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System.Linq;
using eBalanceKit.Models;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.Presentation;

namespace eBalanceKit.Structures {
    /// <summary>
    /// Each instance of this class represents an entry in the navigation tree.
    /// </summary>
    internal class NavigationTreeEntry : NavigationTreeEntryBase {

        internal NavigationTreeEntry() { 
            IsVisible = true; 
        }

        #region properties

        #region ValidationWarning
        /// <summary>
        /// Returns false, if the node itself or at least one of it's child nodes (full recursive) is not valid or true otherwhise.
        /// </summary>
        public bool ValidationWarning {
            get {
                return (_containsValidationWarning ||
                        Children.OfType<NavigationTreeEntry>().Any(child => child.ValidationWarning)) &&
                       !eBalanceKitBusiness.Options.GlobalUserOptions.UserOptions.HideAllWarnings;
            }
            set {
                _containsValidationWarning = value;

                OnPropertyChanged("ValidationWarning");

                // notify property changed for all parent nodes
                var parent = Parent;
                while (parent != null) {
                    ((NavigationTreeEntry)parent).OnPropertyChanged("ValidationWarning");
                    parent = parent.Parent;
                }

            }
        }
        private bool _containsValidationWarning;
        #endregion

        public void UserOptionChanged() {
            OnPropertyChanged("ValidationWarning");
            var parent = Parent;
            while (parent != null) {
                ((NavigationTreeEntry)parent).OnPropertyChanged("ValidationWarning");
                parent = parent.Parent;
            }
        }


        #region ValidationError
        public bool ValidationError {
            get { return _validationError || Children.OfType<NavigationTreeEntry>().Any(child => child.ValidationError); }
            set {
                _validationError = value;

                OnPropertyChanged("ValidationError");

                // notify property changed for all parent nodes
                var parent = Parent;
                while (parent != null) {
                    ((NavigationTreeEntry)parent).OnPropertyChanged("ValidationError");
                    parent = parent.Parent;
                }
            }
        }
        private bool _validationError;
        #endregion

        #region ShowHBSTTransferPanel
        private bool _showHBSTTransferPanel;

        public bool ShowHBSTTransferPanel {
            get { return _showHBSTTransferPanel; }
            set { 
                _showHBSTTransferPanel = value;
                OnPropertyChanged("ShowHBSTTransferPanel");
            }
        }
        #endregion ShowHBSTTransferPanel

        #region ShowInfoPanel
        private bool _showInfoPanel;

        public bool ShowInfoPanel {
            get { return _showInfoPanel; }
            set { 
                _showInfoPanel = value;
                OnPropertyChanged("ShowInfoPanel");
            }
        }
        #endregion ShowInfoShowInfoPanel

        #region ShowBalanceList
        private bool _showBalanceList;

        public bool ShowBalanceList {
            get { return _showBalanceList; }
            set { 
                _showBalanceList = value;
                OnPropertyChanged("ShowBalanceList");
            }
        }
        #endregion ShowBalanceList

        #endregion properties

        #region methods

        public void Validate() {
            if (Model != null) {
                if (Model is DisplayValueTreeModel) {
                    (Model as DisplayValueTreeModel).UpdateChildrenContainsValidationErrorElement();
                    (Model as DisplayValueTreeModel).UpdateChildrenContainsValidationWarningElement();

                    ValidationError = (Model as DisplayValueTreeModel).ContainsValidationErrorElement;
                    ValidationWarning = (Model as DisplayValueTreeModel).ContainsValidationWarningElement;

                } else if (Model is TaxonomyViewModel) {
                    ValidationError = ((TaxonomyViewModel)Model).PresentationTree.ValidationError;
                    ValidationWarning = ((TaxonomyViewModel)Model).PresentationTree.ValidationWarning;
                }
            }

            foreach (NavigationTreeEntry child in Children) child.Validate();
        }

        /// <summary>
        /// Check if the current NavigationTreeEntry has to be shown because of the legal status
        /// </summary>
        public void CheckVisibility(bool showAllEntries = false) {
            
            // The navigation is not yet loaded
            if (PresentationTreeRoots == null || !PresentationTreeRoots.Any()) {
                return;
            }

            Document doc = null;
            if (Model != null) {
                if (Model is DisplayValueTreeModel) {
                    doc = (Model as DisplayValueTreeModel).TaxonomyViewmodel.Document;

                } else if (Model is TaxonomyViewModel) {
                    doc = ((TaxonomyViewModel) Model).Document;
                }
            }

            // There is no document selected or something with the Model went wrong so stop proceeding
            if (doc == null) {
                return;
            }
            
            // per default we show this NavigationTreeEntry
            bool visible = true;

            // check all PresentationTreeRoots - Should be only one
            foreach (Taxonomy.PresentationTree.PresentationTreeNode rootNode in PresentationTreeRoots.Where(root => root is Taxonomy.PresentationTree.PresentationTreeNode)) {

                // Get the root entries according to the PresentationTreeRoots --> done this way because PresentationTreeRoots can not be parsed to eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode
                var rootEntries = from tree in doc.GaapPresentationTrees.Values
                                  where
                                      tree is PresentationTree && tree.RootEntries.First() is PresentationTreeNode &&
                                      tree.RootEntries.Any(x => x.Element.Id == rootNode.Element.Id)
                                  select ((PresentationTree) tree).RootEntries;
                
                // if there are rootentries
                if (rootEntries.Any()) {
                    var relevantRootEntries = rootEntries.First().ToList();
                    foreach (PresentationTreeNode node in relevantRootEntries) {
                        // Update the Visibility of all PresentationTreeNodes
                        node.UpdateVisibility(((PresentationTree)node.PresentationTree).DefaultFilter);
                    
                        // if an root entry is visible the navigationTreeEntry should be visible
                        visible &= node.IsVisible;
                    }


                } else {
                    System.Diagnostics.Debug.WriteLine("rootEntries doesn't contain values - eBalanceKit.Structures.NavigationTreeEntry.CheckVisibility()");
                    // maybe this way?

                    //var xy = rootEntries.Where(x => (x.First() is Taxonomy.PresentationTree.PresentationTreeNode &&
                    //    (x.First() as Taxonomy.PresentationTree.PresentationTreeNode).Element.Id == rootNode.Element.Id)).ToList();
                    //foreach (Taxonomy.PresentationTree.PresentationTreeNode node in xy.First()) {
                    //    visible &= node.IsVisible;
                    //}
                }
                
            }
            // Set the own Visibility (true if at least one of the root entries [there should be only one but OK] is visible)
            IsVisible = visible;
            
        }

        #endregion
       
    }
}
