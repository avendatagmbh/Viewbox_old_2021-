// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;

namespace Taxonomy.Interfaces {
    /// <summary>
    /// XBRL roleType element.
    /// </summary>
    public interface IRoleType : IComparable {
        string Name { get; }
        string Id { get; }
        string RoleUri { get; }
        bool HasPresentationLink { get; }
        bool HasCalculationLink { get; }
        bool HasDefinitionLink { get; }
        XbrlDisplayStyle Style { get; }
    }
}