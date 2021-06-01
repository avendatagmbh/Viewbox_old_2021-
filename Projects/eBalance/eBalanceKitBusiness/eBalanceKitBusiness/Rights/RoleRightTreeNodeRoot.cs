using eBalanceKitBusiness.Manager;
using eBalanceKitResources.Localisation;

namespace eBalanceKitBusiness.Rights {

    /// <summary>
    /// Structure for tree representation of root role rights.
    /// </summary>
    /// <author>Benjamin Held</author>
    /// <since>2011-07-19</since>
    public class RoleRightTreeNodeRoot : RoleRightTreeNode {

        #region constructor
        public RoleRightTreeNodeRoot(Role role) : base(role, null){
            Right = role.GetRootRight();
            foreach (var company in CompanyManager.Instance.Companies) {
                RoleRightTreeNodeCompany companyTreeNode = new RoleRightTreeNodeCompany(role, company, this);
                if(UserManager.Instance.CurrentUser.IsAdmin || companyTreeNode.HasGrantChildren)
                    // only add company when it is visible to the cmpany admin user (for admin all companies are visible) 
                    if (RightManager.RightDeducer.CompanyVisible(company))
                        this.Children.Add(companyTreeNode);
            }
            
        }
        #endregion constructor

        
        #region properties
        public override string HeaderString { get { return ResourcesCommon.RightTreeNodeAllCaption; } }       
        public override bool IsRightAllowed(int i) { return RightManager.RightDeducer.GetRootRight().IsRightAllowed(i); }
        #endregion properties
    }
}
