// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since:  2011-06-25
// copyright 2011 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Taxonomy.Interfaces;
using Taxonomy.Interfaces.PresentationTree;

namespace Taxonomy {

    /// <summary>
    /// Interface for the taxonomy class.
    /// </summary>
    public interface ITaxonomy {
        
        /// <summary>
        /// Path to the taxonomy files (relative to application path).
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Filename of the taxonomy entry point file.
        /// </summary>
        string Filename { get; }

        /// <summary>
        /// Reference to the xbrl schema file.
        /// </summary>
        string SchemaRef { get; }

        /// <summary>
        /// Gets the role with the specified role id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IRoleType GetRole(string id);

        /// <summary>
        /// Checks if the specified role id exits.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool RoleExists(string id);

        /// <summary>
        /// Enumeraion of all available roles.
        /// </summary>
        IEnumerable<IRoleType> Roles { get; }

        /// <summary>
        /// Enumeration of all contained presentation trees.
        /// </summary>
        IEnumerable<IPresentationTree> PresentationTrees { get; }

        /// <summary>
        /// Dictionary of all contained elements, accessed by taxonomy id.
        /// </summary>
        Dictionary<string, IElement> Elements { get; }

        void InitSelectionLists();

        /// <summary>
        /// Returns the presentation tree with the specified rolw URI or null, if no such presentation tree exists.
        /// </summary>
        IPresentationTree GetPresentationTree(string roleUri);
    }
}
