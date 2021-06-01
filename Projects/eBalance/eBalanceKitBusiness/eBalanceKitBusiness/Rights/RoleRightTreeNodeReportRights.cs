using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.Rights;
using eBalanceKitResources.Localisation;

/// <summary>
/// Structure for tree representation of report element role rights.
/// </summary>
/// <author>Benjamin Held</author>
/// <since>2011-07-19</since>
namespace eBalanceKitBusiness.Rights {
    
    public class RoleRightTreeNodeReportRights : RoleRightTreeNode {
      
        #region constructor
        public RoleRightTreeNodeReportRights(Role role, DbRight.ContentTypes specialRight, Document parentDocument, RoleRightTreeNode parent)
            : base(role,parent) {
            this.SpecialRight = specialRight;
            this.ParentDocument = parentDocument;

            Right = role.GetRight(specialRight, parentDocument);
        }
        #endregion constructor

        #region properties
        private Document ParentDocument { get; set; }
        public DbRight.ContentTypes SpecialRight { get; private set; }

        public override string HeaderString {
            get {
                switch (SpecialRight) {
                    case DbRight.ContentTypes.DocumentSpecialRight_Rest:
                        return ResourcesCommon.RightTreeNodeOtherReportsCaption;
                    case DbRight.ContentTypes.DocumentSpecialRight_TransferValueCalculation:
                        return ResourcesCommon.RightTreeNodeReconciliationCaption;
                    default:
                        return "test";
                }
                
            }
        }

        public override bool IsRightAllowed(int i) {
            return RightManager.RightDeducer.GetSpecialRight(ParentDocument, SpecialRight).IsRightAllowed(i);
        }


        #endregion properties
    }
}
