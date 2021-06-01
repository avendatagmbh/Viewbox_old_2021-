using System;
using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Rights {

    /// <summary>
    /// Structure for tree representation of financial year role rights.
    /// </summary>
    /// <author>Benjamin Held</author>
    /// <since>2011-07-19</since>
    public class RoleRightTreeNodeFinancialYear : RoleRightTreeNode {

        #region constructor
        public RoleRightTreeNodeFinancialYear(Role role, FinancialYear fYear, RoleRightTreeNode parent) : base(role,parent) {
            this.FinancialYear = fYear;
            Right = role.GetRight(fYear);
            foreach (var document in DocumentManager.Instance.Documents)
                if (document.Company == fYear.Company && document.FinancialYear == fYear) {
                    RoleRightTreeNodeReport reportTreeNode = new RoleRightTreeNodeReport(role, document, this);
                    if ((UserManager.Instance.CurrentUser.IsAdmin || UserManager.Instance.CurrentUser.IsCompanyAdmin) || reportTreeNode.HasGrantChildren)
                        this.Children.Add(reportTreeNode);
                }
        }
        #endregion constructor

        #region properties
        public override string HeaderString { get { return Convert.ToString(FinancialYear.FYear); } }
        private FinancialYear FinancialYear { get; set; }
        public override bool IsRightAllowed(int i) { return RightManager.RightDeducer.GetRight(this.FinancialYear).IsRightAllowed(i); }
        #endregion properties
    }
}
