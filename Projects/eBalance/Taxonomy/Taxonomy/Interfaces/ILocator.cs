// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-10-15
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
namespace Taxonomy.Interfaces {
    /// <summary>
    /// XBRL locator element.
    /// </summary>
    public interface ILocator {
        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        string Type { get; }

        /// <summary>
        /// Gets or sets the reference target.
        /// </summary>
        string HREF { get; }

        /// <summary>
        /// Gets the filename of the reference target.
        /// </summary>
        string File { get; }

        /// <summary>
        /// Gets the element of the hyperlink reference target.
        /// </summary>
        string Element { get; }

        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <value>The label.</value>
        string Label { get; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        string Title { get; }
    }
}