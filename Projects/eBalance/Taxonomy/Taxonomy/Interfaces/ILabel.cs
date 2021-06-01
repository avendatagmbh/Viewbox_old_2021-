// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Taxonomy.Interfaces {
    public interface ILabel {
        string Type { get; }
        string Content { get; }
        string Id { get; }
        string Role { get; }
        string Language { get; }
        string Caption { get; }
        string Title { get; }
    }
}