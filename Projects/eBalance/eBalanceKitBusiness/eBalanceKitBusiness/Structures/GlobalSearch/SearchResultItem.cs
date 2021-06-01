using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;
using eBalanceKitBusiness.Structures.GlobalSearch.Enums;

namespace eBalanceKitBusiness.Structures.GlobalSearch {
    /// <summary>
    /// Structure to store search items.
    /// </summary>
    public class SearchResultItem : GlobalSearcherTreeNode {

        /// <summary>
        /// Store the path in a list, so we can traverse through all the parent nodes.
        /// </summary>
        public List<ISearchableNode> PresentationTreeEntryPath { get; set; }

        public override string Label { get { return PresentationTreeEntryPath.Last().Element.Label; } }
        public string TaxonomyId { get { return PresentationTreeEntryPath.Last().Element.Id; } }        

        public IValueTreeEntry ValueTreeEntry { get; set; }

        /// <summary>
        /// Path as string. We are using the Element.Label as display string. Have to change it if we want to use balance list entries as well.
        /// </summary>
        public string Path {
            get {
                StringBuilder builder = new StringBuilder();

                foreach (ISearchableNode presentationTreeEntry in PresentationTreeEntryPath) {
                    builder.Append(presentationTreeEntry.Element.Label);
                    builder.Append("/");
                }

                if (builder.Length == 0) {
                    return string.Empty;
                }

                // remove the last '/'
                builder.Remove(builder.Length - 1, 1);
                return builder.ToString();
            }
        }

        #region TopLevel

        public TopLevels TopLevel { get; set; }
        #endregion
    }
}
