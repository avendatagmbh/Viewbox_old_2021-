// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-05
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using eBalanceKitBusiness.Interfaces.PresentationTree;
using eBalanceKitBusiness.Manager;

namespace eBalanceKitBusiness.MappingTemplate {
    public class MappingLineGui : MappingGuiBase {
        #region constructor
        public MappingLineGui(MappingHeaderGui parent) {
            Header = parent;
            TaxonomyIdManager = parent.Mapping.Template.TaxonomyIdManager;
        }
        #endregion

        public override bool IsSelected {
            get { return base.IsSelected; }
            set {
                base.IsSelected = value;
                var node = PresentationTreeNode;
                while (node != null) {
                    node.IsExpanded = true;
                    node = (IPresentationTreeNode)node.Parents.FirstOrDefault();
                }
                Header.IsExpandedAssignments = true;
            }
        }

        public MappingHeaderGui Header { get; set; }
        private TaxonomyIdManager TaxonomyIdManager { get; set; }
        public string AccountLabel { get { return Header.AccountLabel; } }
        public override string AccountNumber { get { return Header.AccountNumber; } }
        public override string AccountName { get { return Header.AccountName; } }
        public decimal Balance { get { return 0; } }



        #region ElementId
        public string ElementId {
            get {
                switch (Type) {
                    case MappingTemplateTypes.None:
                        return Header.Mapping.ElementId;
                    case MappingTemplateTypes.Debitor:
                        return Header.Mapping.DebitElementId;
                    case MappingTemplateTypes.Creditor:
                        return Header.Mapping.CreditElementId;
                    default:
                        throw new Exception("Unknown mapping template type.");
                }
            }
            set {
                switch (Type) {
                    case MappingTemplateTypes.None:
                        Header.Mapping.ElementId = value;
                        break;
                    case MappingTemplateTypes.Debitor:
                        Header.Mapping.DebitElementId = value;
                        break;
                    case MappingTemplateTypes.Creditor:
                        Header.Mapping.CreditElementId = value;
                        break;
                    default:
                        throw new Exception("Unknown mapping template type.");
                }

                OnPropertyChanged("ElementId");
                OnPropertyChanged("ElementLabel");
            }
        }
        #endregion ElementId

        #region Type
        public MappingTemplateTypes Type { get; set; }

        public string TypeDisplayString {
            get {
                switch (Type) {
                    case MappingTemplateTypes.Debitor:
                        return "(S)";
                    case MappingTemplateTypes.Creditor:
                        return "(H)";
                    case MappingTemplateTypes.None:
                        return "";
                    default:
                        throw new Exception("Unknown mapping template type.");
                }
            }
        }
        #endregion Type

        #region ElementLabel
        public string ElementLabel {
            get {
                if (ElementId == null) return "-";
                
                var element = TaxonomyIdManager.GetElement(ElementId);
                if (element != null) return element.Label;
                return "Unbekannte ID: " + ElementId;
            }
        }
        #endregion ElementLabel

        public IPresentationTreeNode PresentationTreeNode { get; internal set; }

        public bool IsEnabled { get { return true; } }
        public bool IsComputed { get { return false; } }
        public IEnumerable<IPresentationTreeNode> Children { get { return null; } }
    }
}