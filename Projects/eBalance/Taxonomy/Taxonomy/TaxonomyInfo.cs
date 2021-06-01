// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy.Enums;
using Taxonomy.Interfaces;

namespace Taxonomy {
    public class TaxonomyInfo : ITaxonomyInfo {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Filename { get; set; }
        public TaxonomyType Type { get; set; }
        public string Version { get; set; }         
        
    }
}