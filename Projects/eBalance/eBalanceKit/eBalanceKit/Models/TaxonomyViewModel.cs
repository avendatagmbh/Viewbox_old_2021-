// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2010-12-08
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using Utils;
using eBalanceKitBusiness.Rights;
using IPresentationTree = eBalanceKitBusiness.IPresentationTree;

namespace eBalanceKit.Models {
    /// <summary>
    /// Model class for all taxonomy views. This model combines all nessesary structures which are needed in the
    /// sevaral taxonomy detail views, e.g. balance sheets (Bilanzen) or income statements (GuV).
    /// </summary>
    public class TaxonomyViewModel : INotifyPropertyChanged {
        internal TaxonomyViewModel(
            Window owner,
            ObjectWrapper<eBalanceKitBusiness.Structures.DbMapping.Document> documentWrapper,
            IPresentationTree presentationTree = null) {
            
            Owner = owner;
            _documentWrapper = documentWrapper;
            _documentWrapper.PropertyChanged += DocumentWrapperPropertyChanged;
            _presentationTree = presentationTree;    
        }

        private void DocumentWrapperPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Value") {
                OnPropertyChanged("Document");
                OnPropertyChanged("PresentationTree");
                OnPropertyChanged("ReportRights");

            }
        }

        #region events
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string property) { if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property)); }
        #endregion events

        #region properties
        //public string RoleURI { get; set; }

        #region RoleURI
        private string _roleURI;

        public string RoleURI {
            get { return _roleURI; }
            set {
                if (_roleURI != value) {
                    _roleURI = value;                
                    if(PresentationTree != null) {
                        
                        foreach(IPresentationTreeNode node in PresentationTree.RootEntries)
                            AddScrollIntoViewRequestedEventHandlers(node);
                    }
                    OnPropertyChanged("RoleURI");
                }
            }
        }

        #region scroll to middle

        /// <summary>
        /// Recursively add all the children the event handler.
        /// </summary>
        /// <param name="node">the node. Because of the child can be balance list entry</param>
        private void AddScrollIntoViewRequestedEventHandlers(IPresentationTreeNode node) {
            node.ScrollIntoViewRequested += NodeOnScrollIntoViewRequested;
            foreach (IPresentationTreeEntry subNode in node.Children) {

                IPresentationTreeNode subNodeAsPresentationTreeNodeTaxonomy = subNode as IPresentationTreeNode;
                if (subNodeAsPresentationTreeNodeTaxonomy == null) {
                    // to be more specific : 
                    //IBalanceListEntry subNodeAsBalanceListEntry = subNode as IBalanceListEntry;
                    //// that can change. If changed make sure the childrens of this data type are checked.
                    //Debug.Assert(subNodeAsBalanceListEntry != null);
                    //subNodeAsBalanceListEntry.ScrollIntoViewRequested += NodeScrollIntoViewRequested;
                    //subNode.ScrollIntoViewRequested += NodeScrollIntoViewRequested;
                    continue;
                }

                AddScrollIntoViewRequestedEventHandlers(subNodeAsPresentationTreeNodeTaxonomy);
            }
        }

        /// <summary>
        /// The event to bring the IPresentationTreeEntry type sender to middle. We search for the TreeViewItem behind that element.
        /// The element must be in the TreeView tree somewhere. Used in MainWindowModel.cs
        /// </summary>
        private void NodeOnScrollIntoViewRequested(IList<ISearchableNode> path) {
            if (path != null) {
                ISearchableNode node = path[path.Count - 1];
                Debug.Assert(node != null);

                for (int i = 0; i < TreeView.Items.Count; i++) {
                    // see the comments inside ElementIdChecker!
                    TreeViewItem item = TreeView.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                    if (item == null) {
                        continue;
                    }
                    TreeViewItem element = ElementIdChecker(item, node.Element.Id);
                    if (element == null) {
                        continue;
                    }
                    element.BringIntoView();
                    element.IsSelected = true;
                }

            }
        }

        /// <summary>
        /// Checks if a TreeViewItem object, that's DataContext is IPresentationTreeEntry have a child with the given element id.
        /// So the object.DataContext.Element.Id == checkElementId. Returns the TreeViewItem.
        /// </summary>
        /// <param name="actualNode">The checked TreeViewItem</param>
        /// <param name="checkedElementId">The Id which we are checking</param>
        /// <returns>null if there is no such child, or the TreeViewItem</returns>
        private TreeViewItem ElementIdChecker(TreeViewItem actualNode, string checkedElementId) {
            ISearchableNode actualPresentationTreeEntry = (actualNode.DataContext as ISearchableNode);
            Debug.Assert(actualPresentationTreeEntry != null);
            // we can't check for child that doesn't have element
            if (actualPresentationTreeEntry.Element == null) {
                return null;
            }

            // found the child
            if (actualPresentationTreeEntry.Element.Id == checkedElementId)
                return actualNode;

            for (int i = 0; i < actualNode.Items.Count; i++) {
                // tricky part : We can't access the child items directly. Need a ContainerGenerator. If we set breakpoint somewhere
                // next to this line, the result is possibly error, because the container only access the active resources.
                // possibly in a stress test it would cause error.
                TreeViewItem child = actualNode.ItemContainerGenerator.ContainerFromIndex(i) as TreeViewItem;
                // maybe the child is not yet set. TODO: Check in a test if this cause a problem or not.
                if (child == null) {
                    continue;
                }
                // check the same thing for every child.
                TreeViewItem ret = ElementIdChecker(child, checkedElementId);
                if (ret == null) {
                    continue;
                }
                return ret;
            }
            return null;
        }

        #endregion scroll to middle

        #endregion RoleURI

        public ReportRights ReportRights {
            get { return Document != null ? Document.ReportRights : null; }
        }

        #region PresentationTree
        private IPresentationTree _presentationTree;
        public IPresentationTree PresentationTree {
            get {
                try {
                    if (_presentationTree != null) return _presentationTree;
                    if (Document == null) return null;
                    if (!Document.GaapPresentationTrees.ContainsKey(RoleURI)) {
                        System.Diagnostics.Debug.WriteLine("Presentation tree not found for role: " + RoleURI);
                        return null;
                    }

                    return Document.GaapPresentationTrees[RoleURI];
                } catch (Exception ex) {
                    System.Diagnostics.Debug.WriteLine("Presentation tree not found for role: " + RoleURI + Environment.NewLine + ex.Message);
                    return null;
                }
            }
        }
        #endregion

        #region Elements
        private Dictionary<string, Taxonomy.IElement> _elements;

        /// <summary>
        /// This dictionary is needed for direct access to the respective taxonomy 
        /// elements, e.g. used for "notes" or "gcd" views.
        /// </summary>
        /// <value>The elements.</value>
        public Dictionary<string, Taxonomy.IElement> Elements {
            get { return _elements; }
            set {
                if (_elements != value) {
                    _elements = value;
                    OnPropertyChanged("Elements");
                }
            }
        }
        #endregion

        #region Document
        private readonly ObjectWrapper<eBalanceKitBusiness.Structures.DbMapping.Document> _documentWrapper;

        public eBalanceKitBusiness.Structures.DbMapping.Document Document {
            get { return _documentWrapper.Value; }
            set { _documentWrapper.Value = value; }
        }
        #endregion Document

        #region Owner
        public Window Owner { get; set; }
        public TreeView TreeView { get; set; }
        #endregion

        //public PresentationTreeFilter Filter { get; set; }
        #endregion properties
    }
}