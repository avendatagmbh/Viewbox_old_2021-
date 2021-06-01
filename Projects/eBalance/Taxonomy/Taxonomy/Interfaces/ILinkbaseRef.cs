// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using Taxonomy.Enums;

namespace Taxonomy.Interfaces {
    public interface ILinkbaseRef {
        string Type { get; }
        string HREF { get; }
        string Title { get; }
        string RoleURI { get; }
        LinkbaseRoles Role { get; }
        string Arcrole { get; }
        string Path { get; }
        string File { get; }
    }
}