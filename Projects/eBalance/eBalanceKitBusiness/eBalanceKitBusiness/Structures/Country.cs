/**************************************************************************************************************
 * author               date            comment
 * ------------------------------------------------------------------------------------------------------------
 * Mirko Dibbert        2011-01-09      initial implementation (based on DbAccess 1.0)
 *************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eBalanceKitBusiness.Structures {
    
    public class Country {

        static Country() {
            Country.Countries = new List<Country> {
                new Country("Deutschland", "DE"),
                //new Country("xxx", "yy"),
            };
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Country"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="iso">The iso.</param>
        public Country(string name, string iso) {
            this.Name = name;
            this.Iso = iso;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the iso.
        /// </summary>
        /// <value>The iso.</value>
        public string Iso { get; set; }

        public override string ToString() {
            return this.Name + " (" + this.Iso + ")";
        }

        public static List<Country> Countries { get; private set; }
    }
}
