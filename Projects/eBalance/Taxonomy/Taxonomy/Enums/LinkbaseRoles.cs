// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Taxonomy.Enums {
    /// <summary>
    /// Enumeration of all avaliable linkbase roles.
    /// </summary>
    public enum LinkbaseRoles {
        /// <summary>
        /// Link may refer to any extended link elements.
        /// </summary>
        Unspecified,

        /// <summary>
        /// Link must contain labelLink elements.
        /// </summary>
        Label,

        /// <summary>
        /// Link must contain definitionLink elements.
        /// </summary>        
        Definition,

        /// <summary>
        /// Link must contain referenceLink elements.
        /// </summary>        
        Reference,

        /// <summary>
        /// Link must contain presentationLink elements.
        /// </summary>        
        Presentation,

        /// <summary>
        /// Link must contain calculationLink elements.
        /// </summary>
        Calculation
    }
}