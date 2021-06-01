// --------------------------------------------------------------------------------
// author: Erno Taba
// since: 2012-11-08
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using federalGazetteBusiness;
using Taxonomy.Interfaces.PresentationTree;
using Utils.Commands;
using System.Windows.Input;
using System.Diagnostics;
using eBalanceKitBusiness.Structures.Presentation;
using federalGazetteBusiness.Structures.Enum;
using federalGazetteBusiness.Structures.Manager;

namespace eBalanceKit.Models.FederalGazette
{
    /// <summary>
    /// FederalGazetteTreeViewModel is a model for Config panel in Federal Gazette Assistant windows. Help to checked items by Company size.
    /// </summary>
    public class FederalGazetteTreeViewModel : Utils.NotifyPropertyChangedBase
    {
        #region Variables and getter/setters
        private IPresentationTree _presTree;
        private FederalGazetteManagerBase _manager;
        private FederalGazetteModel _parent;

        public IPresentationTree PresentationTree
        {
            get { return _presTree; }
            set
            {
                if (_presTree != value)
                {
                    _presTree = value;
                    OnPropertyChanged("PresentationTree");
                }
            }
        }

        private CompanySize _companySize;
        public CompanySize CompanySize
        {
            get { return _companySize; }
            set 
            {
                if (_companySize != value)
                {
                    _companySize = value;
                    OnPropertyChanged("CompanySize");
                }
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="FederalGazetteTreeViewModel"/> class.
        /// </summary>
        /// <param name="pPresTree">Param of the PresentationTree of current dataContex.</param>
        /// <param name="pManager">param of FederalGazetteManager.</param>
        public FederalGazetteTreeViewModel(IPresentationTree pPresTree, FederalGazetteManagerBase pManager, FederalGazetteModel pParent)
        {
            if (pPresTree != null) {
                _presTree = pPresTree;
                (_presTree as PresentationTree).CheckingValueChanged += FederalGazetteTreeViewModel_CheckingValueChanged;
            }

            if(pManager != null)
               _manager = pManager;

            _parent = pParent;
           

            CmdCheckItemsByCompanySize = new DelegateCommand(o => true, CheckItemsByCompanySize);
        }

        void FederalGazetteTreeViewModel_CheckingValueChanged() { _parent.RefreshPreview(FederalGazetteAssistantTabs.TreeView); }

        #endregion

        #region Methods
        /// <summary>
        /// Sets the size of the send company and iterate all of element which in the list whats need for company size.
        /// </summary>
        /// <param name="pSize">Size of the company.</param>
        public void SetCheckedItemsByCompanySize(CompanySize pSize, bool pIsCompanySizeOption = false, bool pIsBalanceList = true)
        {
            //Uncheck all checked items:
            foreach (PresentationTreeNode node in _presTree.Nodes.Where(el => el.isSendAccountBalanceForAllParents))
            {
                if (node != null)
                    node.isSendAccountBalanceForAllParents = false;
            }

            foreach (PresentationTreeNode node in _presTree.Nodes.OfType<PresentationTreeNode>())
            {
                //Set deafult value to true:
                if (pIsCompanySizeOption)
                    node.IsCheckBoxEnabled = true;

                //If current node's id in needed id list, check them:
                if (_manager.TaxonomyElementsByCompanySize[pSize].Any(el => el.Id.Equals(node.Element.Id)))
                {                    
                    if (node.Parents != null)
                        SetSendAccountBalancesParents(node.Parents, pIsCompanySizeOption, (!pIsBalanceList && pSize == CompanySize.Small));

                    var value = eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument.ValueTreeMainFg.GetValue(node.Element.Id);
                    if (value != null) {
                        node.isSendAccountBalanceForAllParents = true;
                        if (pIsCompanySizeOption)
                        {
                            if (!pIsBalanceList 
                                && pSize == CompanySize.Small)
                                continue;
                            
                            node.IsCheckBoxEnabled = false;
                        }
                    }
                }
            }

            //if not automatized checking then we need refresh the preview:
            //if (!pIsCompanySizeOption)
            //    _parent.RefreshPreview(FederalGazetteAssistantTabs.TreeView);
        }

        /// <summary>
        /// Sets the send account balances of parents. Recursive iteration of the current node's parent nodes, and check to all of them.
        /// </summary>
        /// <param name="pParents">The parent list of current node.</param>
        private void SetSendAccountBalancesParents(IEnumerable<IPresentationTreeNode> pParents, bool pIsCompanySizeOption, bool IsEnableCheckBox)
        {      
            foreach (PresentationTreeNode nodeParent in pParents)
            {
                var value = eBalanceKitBusiness.Manager.DocumentManager.Instance.CurrentDocument.ValueTreeMainFg.GetValue(nodeParent.Element.Id);
                if (value == null)
                    continue;

                if (!value.SendAccountBalances)
                {
                    value.SendAccountBalances = true;
                    if (pIsCompanySizeOption) {
                        if (IsEnableCheckBox)
                            continue;
                        
                        nodeParent.IsCheckBoxEnabled = false;
                    }
                }

                if (nodeParent.Parents != null)
                    SetSendAccountBalancesParents(nodeParent.Parents, pIsCompanySizeOption, IsEnableCheckBox);
            }                    
        }

        #region Expand and Collapse all node
        public void ExpandAllNodes()
        {
            foreach (PresentationTreeNode node in _presTree.Nodes.OfType<eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode>())
            {
                node.IsExpanded = true;
            }            
        }

        public void CollapseAllNodes()
        {
            foreach (PresentationTreeNode node in _presTree.Nodes.OfType<eBalanceKitBusiness.Structures.Presentation.PresentationTreeNode>())
            {
                node.IsExpanded = false;
            }
        }
        #endregion
        #endregion

        #region Commands
        public ICommand CmdCheckItemsByCompanySize { get; set; }

        /// <summary>
        /// Check all items whiches need for selectted CompanySize.
        /// </summary>
        /// <param name="pParam">The param is reprezent a CompanySize value in string.</param>
        public void CheckItemsByCompanySize(object pParam)
        {
            if (pParam != null)
            {
                switch (pParam.ToString())
                {
                    case "Small":
                        SetCheckedItemsByCompanySize(CompanySize.Small);
                        break;
                    case "Midsize":
                        SetCheckedItemsByCompanySize(CompanySize.Midsize);
                        break;
                    case "Big":
                        SetCheckedItemsByCompanySize(CompanySize.Big);
                        break;
                }
            }
        }
        #endregion


    }
}
