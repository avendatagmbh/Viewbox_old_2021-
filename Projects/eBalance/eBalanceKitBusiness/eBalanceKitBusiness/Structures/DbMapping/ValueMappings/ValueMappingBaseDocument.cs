using System;
using DbAccess;
using eBalanceKitBusiness.Interfaces;
using Taxonomy;

namespace eBalanceKitBusiness.Structures.DbMapping.ValueMappings {

    /// <summary>
    /// This class is the base class for all document specific value mapping classes. 
    /// Each instance of a this class represents an individual fact of the assigned taxonomy.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-05-14</since>
    public abstract class ValueMappingBaseDocument : ValueMappingBase {

        #region constructor
        protected ValueMappingBaseDocument() { }
        #endregion

        #region properties
        [DbColumn("document_id", AllowDbNull = true, IsInverseMapping = true)]
        public Document Document { get; set; }
        #endregion

        #region methods

        internal static IValueMapping Load(IDatabase conn, Type valueMappingType, Document document, ITaxonomy taxonomy) {
            var root = ValueMappingBase.Load(conn, valueMappingType, taxonomy, "document_id", document.Id, document);
            SetDocumentProperties(document, root as ValueMappingBase);
            
            // save root element, if it has been newly created
            if ((root as ValueMappingBase).Id == 0) conn.DbMapping.Save(root);
            
            return root as IValueMapping;
        }

        private static void SetDocumentProperties(Document document, ValueMappingBase root) {
            (root as IValueMappingDocument).Document = document;
            foreach (var child in root.Children) SetDocumentProperties(document, child);
        }
        
        #endregion methods
    }
}
