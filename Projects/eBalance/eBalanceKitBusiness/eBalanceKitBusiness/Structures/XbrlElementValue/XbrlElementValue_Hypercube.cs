// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2012-02-27
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.ValueTree;

namespace eBalanceKitBusiness.Structures.XbrlElementValue {
    public class XbrlElementValue_Hypercube : XbrlElementValueBase, IValueTreeEntry {
        public XbrlElementValue_Hypercube(IElement element, ValueTreeNode parent)
            : base(element, parent, null) {

            Parent.ValueTree.Document.HyperCubeCollectionChanged += (sender, args) => OnPropertyChanged("");
        }

        public string DisplayString {
            get {
                var cube = Parent.ValueTree.Document.GetHyperCube(Element.Id);
                return cube == null ? "-" : cube.Comment;
            }
        }

        public override bool HasValue { get { return Parent.ValueTree.Document.ExistsHyperCube(Element.Id); } }        
        public override bool IsNumeric { get { return false; } }
        
        public override object Value {
            get { return GetValue(); }
            set { /* not possible */ }
        }
        protected override object GetValue() {
            return Parent.ValueTree.Document.GetHyperCube(Element.Id);
        }

        public override bool IsReportable {
            get { return true; }
            set {
                /* not possible */
            }
        }

        public override string Comment {
            get {
                if (!HasValue) return null;
                var cube = Parent.ValueTree.Document.GetHyperCube(Element.Id);
                return cube.Comment;
            }
            set {
                if (!HasValue) return;
                var cube = Parent.ValueTree.Document.GetHyperCube(Element.Id);
                cube.Comment = value;
            }
        }
        protected override void SetValue(object value) { /* not possible */ }
    }
}