using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using eBalanceKitBusiness.Structures.DbMapping;

namespace eBalanceKitBusiness.Options {
    public interface IDocumentSettings : INotifyPropertyChanged {
        /// <summary>
        /// Hide or show only informations for the current selected legal form.
        /// </summary>
        bool ShowSelectedLegalForm { get; set; }

        /// <summary>
        /// Hide or show only informations for the current selected TypeOperatingResult (GKV/UKV)
        /// </summary>
        bool ShowTypeOperatingResult { get; set; }

        /// <summary>
        /// Hide or show only mandatory postions
        /// </summary>
        bool ShowOnlyMandatoryPostions { get; set; }

        /// <summary>
        /// Hide or show only informations for the current selected taxonomy.
        /// </summary>
        bool ShowSelectedTaxonomy { get; set; }

        User User { get; set; }
        Document Document { get; set; }
        /// <summary>
        /// Save the current configured options in the database.
        /// </summary>
        /// <returns>Was there a change between current options and the stored options?</returns>
        bool SaveConfiguration();
    }
}
