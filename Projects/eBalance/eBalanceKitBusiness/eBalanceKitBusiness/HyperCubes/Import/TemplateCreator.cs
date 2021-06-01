using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.HyperCubes.Import.Templates.TemplateGenerator;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.HyperCubes.Import
{
    public class TemplateCreator : ImportBase
    {
        public TemplateCreator(Document document) : base(document) { init(); }
        public TemplateCreator(Document document, ImportConfig.PossibleHyperCubes hyperCubeName) : base(document, hyperCubeName) { init(); }
        public TemplateCreator(Document document, string elementId) : base(document, elementId) { init(); }
        public TemplateCreator(IHyperCube cube)
            : base(cube)
        {
            //init();
            _templateGenerator = new TemplateGeneratorDb(cube);
        }

        #region TemplateGenerator
        private TemplateGeneratorDb _templateGenerator;
        /// <summary>
        /// Object to access AddRow, AddColumn and more
        /// </summary>
        public TemplateGeneratorDb TemplateGenerator
        {
            get { return _templateGenerator; }
            set
            {
                if (_templateGenerator != value)
                {
                    _templateGenerator = value;
                    //Config.OnPropertyChanged("TemplateGenerator");
                }
            }
        }

        #endregion TemplateGenerator

        /// <summary>
        /// Initializes the TemplateGenerator
        /// </summary>
        private void init()
        {
            _templateGenerator = new TemplateGeneratorDb(cube);
        }


    }
}
