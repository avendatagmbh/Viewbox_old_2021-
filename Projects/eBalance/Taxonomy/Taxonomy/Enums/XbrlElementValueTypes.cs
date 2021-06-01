// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-07-28
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Taxonomy.Enums {
    public enum XbrlElementValueTypes {
        None,
        Abstract,
        Tuple,
        Boolean,
        Int,
        Numeric,
        Monetary,
        Date,
        String,
        SingleChoice,

        /// <summary>
        /// Special choice type, must be manually set since the taxonomy definition does not provide any 
        /// information to distinguish between single and multiple choice elements in the taxonomy.
        /// </summary>
        MultipleChoice,

        HyperCubeContainerItem

    }
}