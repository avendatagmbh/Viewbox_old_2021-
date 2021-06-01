// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy.Enums;

namespace Taxonomy.Interfaces {
    /// <summary>
    /// XBRL arc element.
    /// </summary>
    public interface IArc {
        string Type { get; }
        string Arcrole { get; }
        string From { get; }
        string To { get; }
        string Use { get; }
        string Title { get; }
        int Priority { get; }
        double Order { get; }
        string PreferredLabel { get; }
        double Weight { get; }
        ILocator FromLocator { get; }
        ILocator ToLocator { get; }

        // xbrl dimensions extension
        ContextElement ContextElement { get; }
        bool Usable { get; }
    }
}