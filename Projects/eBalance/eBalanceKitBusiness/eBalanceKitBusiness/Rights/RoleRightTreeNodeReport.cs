using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.Rights;

namespace eBalanceKitBusiness.Rights {

    /// <summary>
    /// Structure for tree representation of report role rights.
    /// </summary>
    /// <author>Benjamin Held</author>
    /// <since>2011-07-19</since>
    public class RoleRightTreeNodeReport : RoleRightTreeNode {

        #region constructor
        public RoleRightTreeNodeReport(Role role, Document report, RoleRightTreeNode parent)
            : base(role,parent) {
            this.Report = report;
            Right = role.GetRight(report);

            RoleRightTreeNodeReportRights specialRightTreeNode1 = new RoleRightTreeNodeReportRights(role, DbRight.ContentTypes.DocumentSpecialRight_TransferValueCalculation, report, this);
            RoleRightTreeNodeReportRights specialRightTreeNode2 = new RoleRightTreeNodeReportRights(role, DbRight.ContentTypes.DocumentSpecialRight_Rest, report, this);
            if ((UserManager.Instance.CurrentUser.IsAdmin || UserManager.Instance.CurrentUser.IsCompanyAdmin) || specialRightTreeNode1.HasGrantChildren)
                this.Children.Add(specialRightTreeNode1);
            if ((UserManager.Instance.CurrentUser.IsAdmin || UserManager.Instance.CurrentUser.IsCompanyAdmin) || specialRightTreeNode2.HasGrantChildren)
                this.Children.Add(specialRightTreeNode2);
        }
        #endregion constructor

        #region properties
        private Document Report { get; set; }
        public override string HeaderString { get { return Report.Name; } }
        public override bool IsRightAllowed(int i) { return RightManager.RightDeducer.GetRight(Report).IsRightAllowed(i); }
        #endregion properites
    }
}
