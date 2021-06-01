using eBalanceKitBusiness.Manager;
using eBalanceKitBusiness.Structures.DbMapping;
using eBalanceKitBusiness.Structures.DbMapping.Rights;

namespace eBalanceKitBusiness.Rights {

    /// <summary>
    /// Structure for tree representation of company role rights.
    /// </summary>
    /// <author>Benjamin Held</author>
    /// <since>2011-07-19</since>
    public class RoleRightTreeNodeCompany : RoleRightTreeNode {
        
        #region constructor
        public RoleRightTreeNodeCompany(Role role, Company company, RoleRightTreeNode parent)
            : base(role, parent) {
            this.Company = company;
            Right = role.GetRight(company);
            foreach (var fYear in Company.VisibleFinancialYears) {
                RoleRightTreeNodeFinancialYear fYearTreeNode = new RoleRightTreeNodeFinancialYear(role, fYear, this);
                if ((UserManager.Instance.CurrentUser.IsAdmin || UserManager.Instance.CurrentUser.IsCompanyAdmin) || fYearTreeNode.HasGrantChildren)
                    Children.Add(fYearTreeNode);
            }
        }
        #endregion constructor

        #region properties
        public Company Company{get;private set;}
        public override string HeaderString { get { return Company.Name; } }
        public override bool IsRightAllowed(int i) {
            //return RightManager.RightDeducer.GetRight(Company).IsRightAllowed(i);
            if (UserManager.Instance.CurrentUser.IsCompanyAdmin)
                return RoleManager.GetUserRole(UserManager.Instance.CurrentUser).GetRight(Company).IsRightAllowed(i);
            else
                return RightManager.RightDeducer.GetRight(Company).IsRightAllowed(i);
        }
        public override bool IsEditAllowed
        {
            get
            {
                if (UserManager.Instance.CurrentUser.IsCompanyAdmin)
                    return RoleManager.GetUserRole(UserManager.Instance.CurrentUser).GetRight(Company).IsRightAllowed(DbRight.RightTypes.Grant);
                else
                    return RightManager.RightDeducer.GetRight(Company).IsRightAllowed(DbRight.RightTypes.Grant);
            }
        }
        #endregion properties

    }
}
