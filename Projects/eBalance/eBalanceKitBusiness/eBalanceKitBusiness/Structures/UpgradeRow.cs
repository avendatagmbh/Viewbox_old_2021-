using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace eBalanceKitBusiness.Structures {
    /// <summary>
    /// That class will be used as DataContext of a row on the welcome dialog's DataGrid.
    /// </summary>
    public class UpgradeRow {

        /// <summary>
        /// Generated Save button's command.
        /// </summary>
        public ICommand SaveCommand { get; set; }

        /// <summary>
        /// Generated Open button's command.
        /// </summary>
        public ICommand OpenCommand { get; set; }

        /// <summary>
        /// The text of the new release. This will be one of ResourceMappingForUpdate.resx's row's value.
        /// </summary>
        public string Value { get; set; }
    }
}
