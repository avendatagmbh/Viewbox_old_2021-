using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness {

    /// <summary>
    /// Base interface for templates.
    /// </summary>
    /// <author>Mirko Dibbert</author>
    /// <since>2011-07-19</since>
    public interface ITemplateBase {
        
        /// <summary>
        /// Name of the template.
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// Comment of the template.
        /// </summary>
        string Comment { get; set; }
        
        /// <summary>
        /// Template as XML data.
        /// </summary>
        string Template { get; set; }

        /// <summary>
        /// Reads the template specific properties from the xml template string.
        /// </summary>
        void ReadTemplateXml();

        /// <summary>
        /// Writes the template specific properties into the xml template string.
        /// </summary>
        /// <returns></returns>
        void  WriteTemplateXml();
    }
}
