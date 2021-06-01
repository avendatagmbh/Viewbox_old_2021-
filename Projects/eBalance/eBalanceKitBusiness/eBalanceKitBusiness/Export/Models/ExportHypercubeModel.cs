// --------------------------------------------------------------------------------
// author: Mirko Dibbert
// since: 2012-03-01
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Utils;
using eBalanceKitBusiness.HyperCubes.Interfaces.Structure;

namespace eBalanceKitBusiness.Export.Models {
    public class ExportHypercubeModel : NotifyPropertyChangedBase {
        public ExportHypercubeModel(IHyperCube cube) {
            Config = new ConfigExport(cube.Document) {ExportType = ExportTypes.Csv};
            Cube = cube;
        }

        public ConfigExport Config { get; set; }
        private IHyperCube Cube { get; set; }

        #region LastExceptionMessage
        private string _lastExceptionMessage;

        public string LastExceptionMessage {
            get { return _lastExceptionMessage; }
            set {
                _lastExceptionMessage = value;
                OnPropertyChanged("LastExceptionMessage");
            }
        }
        #endregion LastExceptionMessage

        #region Export
        public bool? Export() {
            try {
                new HyperCubeExporter(Config).ExportCsv(Cube.Root.Element.Id);
                LastExceptionMessage = null;
                return true;
            } catch (Exception ex) {
                LastExceptionMessage = ex.Message;
                return null;
            }
        }
        #endregion Export
    }
}