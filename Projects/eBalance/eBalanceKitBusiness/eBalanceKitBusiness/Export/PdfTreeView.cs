/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-05-10      initial implementation
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taxonomy;
using eBalanceKitBusiness.Reconciliation;

namespace eBalanceKitBusiness.Export {

    internal class PdfTreeView {

        internal PdfTreeView() {
        }

        public List<PdfTreeView> Children {
            get { return _children; }
            set { _children = value; }
        }
        private List<PdfTreeView> _children = new List<PdfTreeView>();

        public string HeaderNumber { get; set; }
        public string Header { get; set; }
        public string Value { get; set; }
        public string Comment { get; set; }

        public int Level { get; set; }

        private PdfTreeView _parent;

        public PdfTreeView Parent {
            get { return _parent; }
            set {
                _parent = value;
                if (value != null) {
                    value.Children.Add(this);
                }
            }
        }

        public bool IsAccount { get; set; }
        public bool HasValue { get; set; }
        public bool ExportElement {
            get {
                if (this.HasValue) return true;
                foreach (var child in Children) if (child.ExportElement) return true;                
                return false;

            }
        }
        public bool HasMandatoryChild {
            get { return IsMandatoryField || Children.Any(child => child.HasMandatoryChild); }
        }

        public IElement Element { get; set; }

        public PdfTreeView AddChild(IElement element, string header, int ordinal, bool isAccount = false) {
            PdfTreeView child = new PdfTreeView {
                Element = element,
                Header = header,
                HeaderNumber = this.HeaderNumber == null ? ordinal.ToString() : this.HeaderNumber + "." + ordinal,
                Level = this.Level + 1,
                IsAccount = isAccount
            };
            child.Parent = this;
            return child;
        }

        public ReconciliationInfo ReconciliationInfo { get; set; }
        public bool HasComputedValue { get; set; }

        public bool IsMandatoryField { get; set; }
        public bool ExportAccount { get; set; }
    }
}
