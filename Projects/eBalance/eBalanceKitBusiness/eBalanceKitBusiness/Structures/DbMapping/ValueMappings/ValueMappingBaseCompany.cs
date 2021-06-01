using System;
using DbAccess;
using eBalanceKitBusiness.Interfaces;
using Taxonomy;

namespace eBalanceKitBusiness.Structures.DbMapping.ValueMappings {

    /// <summary>
    /// This class is the base class for all company specific value mapping classes. 
    /// Each instance of a this class represents an individual fact of the assigned taxonomy.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-14</since>
    public abstract class ValueMappingBaseCompany : ValueMappingBase {

        #region constructor
        protected ValueMappingBaseCompany() { }
        #endregion

        #region properties
        [DbColumn("company_id", AllowDbNull = true, IsInverseMapping = true)]
        public Company Company { get; set; }
        #endregion

        #region methods

        internal static IValueMapping Load(IDatabase conn, Type valueMappingType, Company company, ITaxonomy taxonomy) {
            var root = ValueMappingBase.Load(conn, valueMappingType, taxonomy, "company_id", company.Id, company);
            SetCompanyProperties(company, root as ValueMappingBase);

            // save root element, if it has been newly created
            if ((root as ValueMappingBase).Id == 0) conn.DbMapping.Save(root);
            
            return root as IValueMapping;
        }

        private static void SetCompanyProperties(Company company, ValueMappingBase root) {
            (root as IValueMappingCompany).Company = company;
            foreach (var child in root.Children) SetCompanyProperties(company, child);
        }

        #endregion methods
    }
}
