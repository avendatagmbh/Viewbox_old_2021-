using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using SystemDb.Compatibility;
using ProjectDb.Tables;

namespace ViewBuilderBusiness.Structures {
    
    /// <summary>
    /// This class represents a parsed view script.
    /// </summary>
    public class ViewInfo {

        public ViewInfo() {
            this.Name = string.Empty;
            this.Description = string.Empty;
            this.ColumnDictionary = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
            this.Indizes = new List<string>();
            this.Statements = new List<string>();
            this.OptimizeCriterias = new OptimizeCriterias();
            this.Tables = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            Languages = new List<string>();
        }

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>
        public Viewscript View { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the dictionary for the column descriptions.
        /// </summary>
        /// <value>The column dict.</value>
        public Dictionary<string, Dictionary<string,string> > ColumnDictionary { get; private set; }

        /// <summary>
        /// Gets or sets the indizes.
        /// </summary>
        /// <value>The indizes.</value>
        public List<string> Indizes { get; private set; }

        /// <summary>
        /// Gets or sets the statements.
        /// </summary>
        /// <value>The statements.</value>
        public List<string> Statements { get; private set; }

        /// <summary>
        /// Gets or sets the optimize criterias.
        /// </summary>
        /// <value>The optimize criterias.</value>
        public OptimizeCriterias OptimizeCriterias { get; private set; }

        /// <summary>
        /// Gets or sets the tables.
        /// </summary>
        /// <value>The tables.</value>
        public Dictionary<string, long> Tables { get; private set; }

        public List<string> Languages { get; private set; } 
        /// <summary>
        /// Parses the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        public void Parse(string script) {
        }

        public string CompleteStatement { get; set; }
    }
}
