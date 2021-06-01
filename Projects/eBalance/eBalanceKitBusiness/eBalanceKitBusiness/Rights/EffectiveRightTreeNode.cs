namespace eBalanceKitBusiness.Rights {

    /// <summary>
    /// Structure for tree representation of effective user rights.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-08-24</since>
    public class EffectiveUserRightTreeNode : RightTreeNodeBase {
        
        internal EffectiveUserRightTreeNode(Right right, string header, EffectiveRightDescriptor descriptor) {
            Right = right;
            _headerString = header;
            Descriptor = descriptor;

            // set tooltip
            
        }

        internal EffectiveRightDescriptor Descriptor { get; set; }

        private string _headerString;
        public override string HeaderString { get { return _headerString; } }
        
        public override bool IsEditAllowed { get { return true; } }

        private string _toolTip;
        public override string ToolTip {
            get {
                if (_toolTip == null) {
                    if (Descriptor != null) _toolTip = Descriptor.ToolTipString;
                }
                return _toolTip;
            }
            set {
                _toolTip = value;
            }
        }

        public string GetToolTipForType(eBalanceKitBusiness.Structures.DbMapping.Rights.DbRight.RightTypes type) { return Descriptor.GetToolTipForType(type); }
    }
}
