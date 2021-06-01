using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using SystemDb;
using SystemDb.Compatibility.Viewbuilder.OptimizationRelated;

namespace ViewBuilder.Models {
    public class ManageLayersModel {

        #region Constructor
        public ManageLayersModel(Layers layers, ILanguageCollection languages) {
            Layers = new Layers(layers, languages);
            _languages = languages;
            EditTextLayers = new List<Layer>() {
                Layers.TypeToLayer[OptimizationType.System],
                Layers.TypeToLayer[OptimizationType.SplitTable],
                Layers.TypeToLayer[OptimizationType.IndexTable],
                Layers.TypeToLayer[OptimizationType.SortColumn],
            };

        }
        #endregion Constructor

        #region Properties
        private ILanguageCollection _languages;
        public bool Save { get; private set; }
        public Layers Layers { get; private set; }
        public List<Layer> EditTextLayers { get; private set; }
        #endregion Properties

        #region Methods
        public void Apply() {
            Save = true;
        }
        #endregion Methods

        public void SetupColumns(DataGrid dataGrid) {
            //Setup columns
            List<EditOptimizationsModel.TempColumn> columns = new List<EditOptimizationsModel.TempColumn>() {
                new EditOptimizationsModel.TempColumn() { Header = "Ebene", PropertyPath = "Group.ReadableTypeString", IsReadOnly = true}
            };
            for (int i = 0; i < _languages.Count; ++i)
                columns.Add(new EditOptimizationsModel.TempColumn() { PropertyPath = string.Format("Descriptions[{0}]", i), Header = _languages.ElementAt(i).LanguageName });

            EditOptimizationsModel.AddColumnsToDataGrid(dataGrid, columns);
        }
    }
}
