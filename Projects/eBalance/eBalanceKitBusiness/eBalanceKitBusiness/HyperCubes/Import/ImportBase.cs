using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.HyperCubes.Structures;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.HyperCubes.Import
{
    public abstract class ImportBase : NotifyPropertyChangedBase
    {
        protected IHyperCube cube;
        protected Document doc;

        private void LoadHyperCube(ImportConfig.PossibleHyperCubes hyperCubeName) { cube = doc.GetHyperCube(ImportConfig.HyperCubeDictionary[hyperCubeName]); }

        private void LoadHyperCube(string elementId)
        {
            cube = doc.GetHyperCube(elementId);
            /*
            eBalanceKitBusiness.Interfaces.PresentationTree.IPresentationTreeNode node = null;
            foreach (IPresentationTree ptree in doc.GaapPresentationTrees.Values)
            {
                if (ptree.HasNode(elementId))
                {
                    node = ptree.GetNode(elementId) as eBalanceKitBusiness.Interfaces.PresentationTree.IPresentationTreeNode;
                    break;
                }
            }
            if (node != null)
            {
                //node.
                cube = new HyperCube(doc, node);
            }
            */
        }

        protected ImportBase(Document document) {
            doc = document;
            //doc.LoadHyperCubes();
        }

        protected ImportBase(Document document, string elementId)
            : this(document)
        {

            // prepare all infos
            //init();
            
            cube = doc.GetHyperCube(elementId);
            
            //LoadHyperCube(elementId);
        }

        protected ImportBase(Document document, ImportConfig.PossibleHyperCubes hyperCubeName)
            : this(document, ImportConfig.HyperCubeDictionary[hyperCubeName])
        {
            //Config = new ImportConfig { ElementId = document.TaxonomyIdManager.GetId(ImportConfig.HyperCubeDictionary[hyperCubeName]).ToString() };
            
            // prepare all infos
            //init();
        }

        /// <summary>
        /// Create an instance of Importer.
        /// </summary>
        /// <param name="cube">Loads all required informations from the cube.</param>
        protected ImportBase(IHyperCube cube)
        //    : this(cube.Document) 
        { this.cube = cube; }

    }
}
