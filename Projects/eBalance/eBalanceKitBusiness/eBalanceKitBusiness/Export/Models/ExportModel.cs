// --------------------------------------------------------------------------------
// author: Sebastian Vetter, Mirko Dibbert
// since: 2012-02-10
// copyright 2012 AvenDATA GmbH
// --------------------------------------------------------------------------------
using System;
using Utils;

namespace eBalanceKitBusiness.Export.Models
{
    public class ExportModel : NotifyPropertyChangedBase
    {
        public ExportModel(Structures.DbMapping.Document document) { Config = new ConfigExport(document) {ExportType = ExportTypes.Pdf}; }

        public ConfigExport Config { get; set; }

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
                switch (Config.ExportType) {
                    case ExportTypes.Pdf:
                        new PdfExporter().Export(Config);
                        LastExceptionMessage = null;
                        return true;
                    
                    case ExportTypes.Csv:
                        new CsvExport().Export(Config);
                        LastExceptionMessage = null;
                        return true;
                    
                    case ExportTypes.Xbrl:
                        XbrlExporter.ExportXbrl(Config.Filename, Config.Document);
                        LastExceptionMessage = null;
                        return true;
                    
                    default:
                        return false;
                }

            } catch (Exception ex) {
                LastExceptionMessage = ex.Message;
                return null;
            }
        }
        #endregion Export
    }
}
