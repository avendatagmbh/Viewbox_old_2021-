// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-19
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Taxonomy;
using Utils;
using eBalanceKitBusiness.Structures.DbMapping.MappingTemplate;

namespace eBalanceKitBusiness.MappingTemplate {
    public class AssignmentConflict : NotifyPropertyChangedBase {

        public AssignmentConflict(IElement element, MappingTemplateLine item, string newValue, TemplateAssignmentPosition position, TemplateAssignmentConfictResolveMode mode) {
            Element = element;
            Item = item;
            NewValue = newValue;
            Position = position;
            Mode = mode;
        }

        public IElement Element { get; private set; }
        public MappingTemplateLine Item { get; private set; }
        public string NewValue { get; private set; }
        public TemplateAssignmentPosition Position { get; private set; }
        
        public IElement OldElement {
            get {
                if (Item.CreditElement != null) {
                    return Item.CreditElement;
                }
                if (Item.DebitElement != null) {
                    return Item.DebitElement;
                }
                return null;
            }
        }

        public string PositionString {
            get {
                switch (Position) {
                    case TemplateAssignmentPosition.Debit:
                        return "D: ";
                    case TemplateAssignmentPosition.Credit:
                        return "C: ";
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private TemplateAssignmentConfictResolveMode _mode;
        public TemplateAssignmentConfictResolveMode Mode {
            get { return _mode; }
            set { _mode = value; }
        }

        #region IsSelected
        private bool _isSelected;
        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }
        #endregion
    }
}