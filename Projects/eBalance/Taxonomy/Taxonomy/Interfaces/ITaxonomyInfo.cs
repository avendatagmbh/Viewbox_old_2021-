// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-11-09
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy.Enums;

namespace Taxonomy.Interfaces {
    public interface ITaxonomyInfo {
        string Name { get; }
        string Path { get; }
        string Filename { get; }
        TaxonomyType Type { get; }
        string Version { get; }         
    }
}