using System.Collections.ObjectModel;
using ViewValidatorLogic.Structures.InitialSetup;

namespace ViewValidator.Models {
    public class SettingsModel {
        #region Constructor
        public SettingsModel(ValidationSetup setup) {
            this.Setup = setup;
        }

        #endregion
        #region Properties
        public ObservableCollection<TableMapping> TableMappings{
            get { return Setup.TableMappings; }
        }
        public int ErrorLimit {
            get { return Setup.ErrorLimit; }
            set { Setup.ErrorLimit = value; }
        }
        private ValidationSetup Setup { get; set; }
        #endregion
    }
}
